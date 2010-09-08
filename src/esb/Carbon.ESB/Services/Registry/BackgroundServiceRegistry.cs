using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.RuntimeServices;
using Kharbon.Core;
using Carbon.Core.Builder;

namespace Carbon.ESB.Services.Registry
{
    /// <summary>
    /// Registry for holding all of the background services that the infrastructure will use.
    /// </summary>
    public class BackgroundServiceRegistry : IBackgroundServiceRegistry
    {
        private readonly IObjectBuilder m_builder;
        private List<ContextBackgroundService> m_services = null;
        private IMessageBus m_message_bus;

        public bool IsRunning
        {
            get;
            private set;
        }

        public BackgroundServiceRegistry(IObjectBuilder builder)
        {
            m_builder = builder;

            if (m_services == null)
                m_services = new List<ContextBackgroundService>();
        }

        public void Dispose()
        {
            this.Stop();
        }

        public void Start()
        {
            if (IsRunning)
                return;

            if (m_services != null)
                if (m_services.Count > 0)
                    foreach (var service in m_services)
                    {
                        service.Bus = m_message_bus;
                        service.BackgroundServiceError += ServiceError;
                        service.BackgroundServiceStarted += ServiceStarted;
                        service.BackgroundServiceStopped += ServiceStopped;
                        service.Start();
                    }

            IsRunning = true;
        }

        public void Stop()
        {

            if (m_services != null)
                if (m_services.Count > 0)
                    foreach (var service in m_services)
                    {
                        service.Stop();
                        service.BackgroundServiceError -= ServiceError;
                        service.BackgroundServiceStarted -= ServiceStarted;
                        service.BackgroundServiceStopped -= ServiceStopped;
                    }

            IsRunning = false;
        }

        public void SetMessageBus(IMessageBus bus)
        {
            this.m_message_bus = bus;
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
                    var services = this.FindAllBackgroundServiceConfigurations(asm);
                    foreach (var service in services)
                        this.Register(service);
                }
                catch
                {
                    continue;
                }
            }

        }

        public ReadOnlyCollection<ContextBackgroundService> GetAllItems()
        {
            return m_services.AsReadOnly();
        }

        public void Register(ContextBackgroundService item)
        {
            if (item == null)
                return;

            if (!m_services.Contains(item))
                m_services.Add(item);
        }

        public void Remove(ContextBackgroundService item)
        {
            if (item == null)
                return;

            if (m_services.Contains(item))
            {
                if (item.IsRunning)
                {
                    item.Stop();
                    m_services.Add(item);
                }
            }
        }

        public ContextBackgroundService Find(string id)
        {
            var service = m_services.Find(f => f.Name.Trim().ToLower() == id.Trim().ToLower());
            return service;
        }

        private void ServiceStopped(object sender, BackGroundServiceEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI), new Envelope(e.Message));
        }

        private void ServiceStarted(object sender, BackGroundServiceEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI), new Envelope(e.Message));
        }

        private void ServiceError(object sender, BackGroundServiceErrorEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI), new Envelope(e.Message + " " + e.Exception.ToString()));
        }

        private ContextBackgroundService[] FindAllBackgroundServiceConfigurations(Assembly assembly)
        {
            var services = new List<ContextBackgroundService>();

            var reflection = m_builder.Resolve<IReflection>();
            var serviceConfigurations =
                reflection.FindConcreteTypesImplementingInterfaceAndBuild(typeof(IServiceConfigurationStrategy),
                                                                          assembly);

            foreach (var serviceConfiguration in serviceConfigurations)
            {
                var instance = reflection.BuildInstance(serviceConfiguration.GetType().AssemblyQualifiedName);

                if (instance == null) continue;

                var strategy = instance as IServiceConfigurationStrategy;
                strategy.ObjectBuilder = m_builder;
                strategy.Bus = this.m_message_bus;
                var service = strategy.Configure();

                if (service == null) continue;

                services.Add(service);
            }


            return services.ToArray();

        }

        ~BackgroundServiceRegistry()
        {
            if (m_services != null)
            {
                if (this.IsRunning)
                    this.Stop();

                m_services.Clear();
                m_services = null;
            }
        }

    }
}