using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Channel.Impl.Queue;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.Components.Service;
using Carbon.Core.Stereotypes.For.Components.Service.Impl;

namespace Carbon.Core.Registries.For.ServiceEndpoints
{
    public class ServiceEndpointRegistry : IServiceEndpointRegistry
    {
        private readonly IReflection m_reflection;
        private readonly IChannelRegistry m_channel_registry;
        private readonly IAdapterMessagingTemplate m_adapter_messaging_template;
        private static List<IServiceActivator> m_items = null;
        private object m_add_item_lock = new object();
        private object m_remove_item_lock = new object();
        private object m_clear_items_lock = new object();

        public ServiceEndpointRegistry(IReflection reflection,
                                       IChannelRegistry channelRegistry,
                                       IAdapterMessagingTemplate adapterMessagingTemplate)
        {
            m_reflection = reflection;
            m_channel_registry = channelRegistry;
            m_adapter_messaging_template = adapterMessagingTemplate;

            if (m_items == null)
                m_items = new List<IServiceActivator>();
        }

        public ReadOnlyCollection<IServiceActivator> GetAllItems()
        {
            return m_items.AsReadOnly();
        }

        public void Register(IServiceActivator item)
        {
            if (item == null)
                return;

            try
            {
                if (!m_items.Contains(item))
                    lock (m_add_item_lock)
                    {
                        this.BindEndpointEvents(item);
                        m_items.Add(item);
                    }
            }
            catch
            {
                throw;
            }
        }

        public void Remove(IServiceActivator item)
        {
            try
            {
                if (!m_items.Contains(item))
                    lock (m_remove_item_lock)
                    {
                        this.UnBindEndpointEvents(item);
                        m_items.Remove(item);
                    }
            }
            catch
            {
                throw;
            }
        }

        public IServiceActivator Find(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Scan(params string[] assemblyName)
        {
            foreach (var name in assemblyName)
                try
                {
                    this.Scan(Assembly.Load(name));
                }
                catch
                {
                    continue;
                }

        }

        public void Scan(params Assembly[] assembly)
        {
            foreach (var asm in assembly)
            {
                try
                {
                    var endpoints = this.FindAllServiceEndpoints(asm);

                    foreach (var endpoint in endpoints)
                    {
                        try
                        {
                            var activator = this.Configure(endpoint);
                            if (activator != null)
                                this.Register(activator);
                        }
                        catch
                        {
                            continue;
                        }

                    }
                }
                catch
                {
                    continue;
                }
            }

        }

        private Type[] FindAllServiceEndpoints(Assembly assembly)
        {
            var endpoints = new List<Type>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract)
                    if (type.GetCustomAttributes(typeof(ServiceAttribute), true).Length > 0)
                        if (!endpoints.Contains(type))
                            endpoints.Add(type);
            }

            return endpoints.ToArray();
        }

        private IServiceActivator Configure(Type endpoint)
        {
            IServiceActivator activator = null;
            var attributes = endpoint.GetCustomAttributes(typeof(ServiceAttribute), true);

            if (attributes.Length > 0)
            {
                activator = new ServiceActivator(m_channel_registry);

                var channelName = ((ServiceAttribute)attributes[0]).InputChannel;

                // define the channel for accepting the message:
                var channel = m_channel_registry.FindChannel(channelName);

                if (channel is NullChannel)
                {
                    channel = new QueueChannel(channelName);
                    m_channel_registry.RegisterChannel(channel);
                }

                var instance = m_reflection.BuildInstance(endpoint);

                activator.SetInputChannel(channel);
                activator.SetServiceInstance(instance);
            }
            else
            {
                throw new ArgumentException(string.Format("The endpoint '{0}' was configured without an input channel name. Skipped registration..", endpoint.FullName));
            }

            return activator;
        }

        private void BindEndpointEvents(IServiceActivator endpointActivator)
        {
            endpointActivator.ServiceActivatorBeginInvoke += EndpointBeginInvoke;
            endpointActivator.ServiceActivatorEndInvoke += EndpointEndInvoke;
            endpointActivator.ServiceActivatorError += EndpointInvokeError;
        }

        private void UnBindEndpointEvents(IServiceActivator endpointActivator)
        {
            endpointActivator.ServiceActivatorBeginInvoke -= EndpointBeginInvoke;
            endpointActivator.ServiceActivatorEndInvoke -= EndpointEndInvoke;
            endpointActivator.ServiceActivatorError -= EndpointInvokeError;
        }

        private void EndpointInvokeError(object sender, ServiceActivatorErrorEventArgs e)
        {
            m_adapter_messaging_template.DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI), new Envelope(e.ToString()));
        }

        private void EndpointEndInvoke(object sender, ServiceActivatorEndInvokeEventArgs e)
        {
            m_adapter_messaging_template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(e.Message));
        }

        private void EndpointBeginInvoke(object sender, ServiceActivatorBeginInvokeEventArgs e)
        {
            m_adapter_messaging_template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(e.Message));
        }

        ~ServiceEndpointRegistry()
        {
            if (m_items != null)
            {
                foreach (var endpointActivator in m_items)
                {
                    try
                    {
                        UnBindEndpointEvents(endpointActivator);
                    }
                    catch
                    {
                        continue;
                    }
                }

                lock (m_clear_items_lock)
                {
                    m_items.Clear();
                    m_items = null;
                }
            }
        }
    }
}