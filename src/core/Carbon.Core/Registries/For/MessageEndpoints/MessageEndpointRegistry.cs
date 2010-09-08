using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Channel.Impl.Queue;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Subscription;
using Carbon.Core.Builder;

namespace Carbon.Core.Registries.For.MessageEndpoints
{
    public class MessageEndpointRegistry : IMessageEndpointRegistry
    {
        private readonly IObjectBuilder m_builder;
        private static Queue<IMessageEndpointActivator> m_items = null;
        private object m_lock = new object();

        #region -- events --

        /// <summary>
        /// Event that is triggered when the component has started to invoke a method matching the message.
        /// </summary>
        public event EventHandler<MessageEndpointActivatorBeginInvokeEventArgs> MessageEndpointActivatorBeginInvoke;

        /// <summary>
        /// Event that is triggered when the component has finished invoking a method matching the message.
        /// </summary>
        public event EventHandler<MessageEndpointActivatorEndInvokeEventArgs> MessageEndpointActivatorEndInvoke;

        /// <summary>
        /// Event that is triggered when the component has generated an error invoking a method matching the message.
        /// </summary>
        public event EventHandler<MessageEndpointActivatorErrorEventArgs> MessageEndpointActivatorError;

        #endregion

        public MessageEndpointRegistry(IObjectBuilder container)
        {
            m_builder = container;
            if (m_items == null)
                m_items = new Queue<IMessageEndpointActivator>();
        }

        public ReadOnlyCollection<IMessageEndpointActivator> GetAllItems()
        {
            return new List<IMessageEndpointActivator>(m_items).AsReadOnly();
        }

        public void Register(IMessageEndpointActivator item)
        {
            if (item == null)
                return;

            lock (m_lock)
            {
                // the message endpoint can have the same instance, but the message can come in over different input channels:
                var isFound = false;
                foreach (var endpointActivator in m_items)
                {
                    if (endpointActivator.InputChannel.Name.Trim().ToLower() != item.InputChannel.Name.Trim().ToLower()) continue;
                    isFound = true;
                    break;
                }

                if (isFound) return;

                this.BindEndpointEvents(item);
                m_items.Enqueue(item);
            }


        }

        public void Remove(IMessageEndpointActivator item)
        {
            try
            {
                if (m_items.Contains(item))
                    lock (m_lock)
                    {
                        this.UnBindEndpointEvents(item);
                        //m_items.Dequeue(item);
                    }
            }
            catch
            {
                throw;
            }
        }

        public IMessageEndpointActivator Find(Guid id)
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
                    var endpoints = this.FindAllMessageEndpoints(asm);

                    foreach (var endpoint in endpoints)
                    {
                        try
                        {
                            var activator = this.Configure(endpoint);
                            if (activator != null)
                            {
                                //this.BindEndpointEvents(activator);
                                //activators.Add(activator);
                                this.Register(activator);
                            }

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

        /// <summary>
        /// This will create a messaging endpoint from the information and register it for invocation.
        /// </summary>
        /// <param name="inputChannel">Name of the channel where the message will be received from.</param>
        /// <param name="outputChannel">Name of the channel where the message will be sent to (optional)</param>
        /// <param name="methodName">Name of the method on the endpoint(component) that will process the message (optional)</param>
        /// <param name="endpoint">Instance of the endpoint that will handle the message.</param>
        public void CreateEndpoint(string inputChannel, string outputChannel, string methodName, object endpoint)
        {

            this.BuildChannels(inputChannel, outputChannel);

            var activator = m_builder.Resolve<IMessageEndpointActivator>();
            activator.ActivationStyle = EndpointActivationStyle.ActivateOnMessageSent;
            activator.SetInputChannel(inputChannel);
            activator.SetOutputChannel(outputChannel);
            activator.SetEndpointInstance(endpoint.GetType());

            if (!string.IsNullOrEmpty(methodName))
                activator.SetEndpointInstanceMethodName(methodName);

            this.Register(activator);

        }

        public virtual IMessageEndpointActivator ConfigureFromSubscription(ISubscription subscription)
        {
            IMessageEndpointActivator activator = null;

            // first look to see if it has already been configured
            foreach (var endpointActivator in m_items)
            {
                if (endpointActivator.EndpointInstance.GetType().AssemblyQualifiedName.Trim().ToLower() !=
                    subscription.Component.Trim().ToLower()) continue;
                activator = endpointActivator;
                break;
            }

            if (activator != null)
            {
                activator.SetEndpointInstanceMethodName(subscription.MethodName);
                return activator;
            }

            #region --  not found, create from subscription and register --
            var instance = m_builder.Resolve<IReflection>().BuildInstance(subscription.Component);

            if (instance == null)
                return activator;

            activator = m_builder.Resolve<IMessageEndpointActivator>();

            //if (typeof(IMessageBusService).IsAssignableFrom(instance.GetType()))
            //    ((IMessageBusService)instance).Bus = this.m_message_bus;

            // define the channel for accepting the message based on the subscription:
            var channel = m_builder.Resolve<IChannelRegistry>().FindChannel(subscription.Channel);

            if (channel is NullChannel)
            {
                channel = new QueueChannel(subscription.Channel);
                m_builder.Resolve<IChannelRegistry>().RegisterChannel(channel);
            }

            activator.SetInputChannel(channel);
            activator.SetEndpointInstance(instance);
            activator.SetEndpointInstanceMethodName(subscription.MethodName);

            this.Register(activator);
            #endregion

            return activator;
        }

        /// <summary>
        /// This will activate an endpoint within the registry for processing a message via a subscription reference.
        /// </summary>
        /// <param name="subscription"><seealso cref="IEnvelope"/>asubscription that defines how to route the message to the endpoint.</param>
        /// <param name="envelope"><seealso cref="ISubscription"/>message to process.</param>
        /// <returns></returns>
        public IEnvelope ActivateEndpointFromSubscription(ISubscription subscription, IEnvelope envelope)
        {
            var activator = this.ConfigureFromSubscription(subscription);
            envelope = activator.InvokeEndpoint(envelope);
            return envelope;
        }

        /// <summary>
        /// This will activate an endpoint within the registry for processing a message.
        /// </summary>
        /// <param name="activator"><seealso cref="IMessageEndpointActivator"/>activator that will process the message</param>
        /// <param name="envelope"><seealso cref="IEnvelope"/>message to process.</param>
        /// <returns></returns>
        public IEnvelope ActivateEndpoint(IMessageEndpointActivator activator, IEnvelope envelope)
        {
            envelope = activator.InvokeEndpoint(envelope);
            return envelope;
        }

        //public void SetMessageBus(IMessageBus bus)
        //{
        //    this.m_message_bus = bus;
        //}

        private Type[] FindAllMessageEndpoints(Assembly assembly)
        {
            var endpoints = new List<Type>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract)
                    if (type.GetCustomAttributes(typeof(MessageEndpointAttribute), true).Length > 0)
                        if (!endpoints.Contains(type))
                            endpoints.Add(type);
            }

            return endpoints.ToArray();
        }

        private IMessageEndpointActivator Configure(Type endpoint)
        {
            IMessageEndpointActivator activator = null;
            var attributes = endpoint.GetCustomAttributes(typeof(MessageEndpointAttribute), true);

            if (attributes.Length > 0)
            {
                activator = m_builder.Resolve<IMessageEndpointActivator>();

                var channelName = ((MessageEndpointAttribute)attributes[0]).InputChannel;

                // define the channel for accepting the message:
                var channel = m_builder.Resolve<IChannelRegistry>().FindChannel(channelName);

                if (channel is NullChannel)
                {
                    channel = new QueueChannel(channelName);
                    m_builder.Resolve<IChannelRegistry>().RegisterChannel(channel);
                }

                var instance = m_builder.Resolve<IReflection>().BuildInstance(endpoint);

                //if (typeof(IMessageBusService).IsAssignableFrom(instance.GetType()))
                //    ((IMessageBusService)instance).Bus = m_message_bus;

                activator.SetInputChannel(channel);
                activator.SetEndpointInstance(instance);
            }
            else
            {
                throw new ArgumentException(string.Format("The endpoint '{0}' was configured without an input channel name. Skipped registration..", endpoint.FullName));
            }

            return activator;
        }

        private void BindEndpointEvents(IMessageEndpointActivator endpointActivator)
        {
            endpointActivator.MessageEndpointActivatorBeginInvoke += EndpointBeginInvoke;
            endpointActivator.MessageEndpointActivatorEndInvoke += EndpointEndInvoke;
            endpointActivator.MessageEndpointActivatorError += EndpointInvokeError;
        }

        private void UnBindEndpointEvents(IMessageEndpointActivator endpointActivator)
        {
            endpointActivator.MessageEndpointActivatorBeginInvoke -= EndpointBeginInvoke;
            endpointActivator.MessageEndpointActivatorEndInvoke -= EndpointEndInvoke;
            endpointActivator.MessageEndpointActivatorError -= EndpointInvokeError;
        }

        private void BuildChannels(string inputChannel, string outputChannel)
        {
            var registry = m_builder.Resolve<IChannelRegistry>();

            if (registry.FindChannel(inputChannel) is NullChannel)
                registry.RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                if (registry.FindChannel(outputChannel) is NullChannel)
                    registry.RegisterChannel(outputChannel);
        }

        private void EndpointInvokeError(object sender, MessageEndpointActivatorErrorEventArgs e)
        {
            OnEndpointError(e);
        }

        private void OnEndpointError(MessageEndpointActivatorErrorEventArgs endpointActivatorErrorEventArgs)
        {
            EventHandler<MessageEndpointActivatorErrorEventArgs> evt =
                this.MessageEndpointActivatorError;

            if (evt != null)
                evt(this, endpointActivatorErrorEventArgs);
        }

        private void EndpointEndInvoke(object sender, MessageEndpointActivatorEndInvokeEventArgs e)
        {
            OnEndpointEndInvoke(e);
        }

        private void OnEndpointEndInvoke(MessageEndpointActivatorEndInvokeEventArgs endpointActivatorEndInvokeEventArgs)
        {
            EventHandler<MessageEndpointActivatorEndInvokeEventArgs> evt =
                this.MessageEndpointActivatorEndInvoke;

            if (evt != null)
                evt(this, endpointActivatorEndInvokeEventArgs);
        }

        private void EndpointBeginInvoke(object sender, MessageEndpointActivatorBeginInvokeEventArgs e)
        {
            OnEndpointBeginInvoke(e);
        }

        private void OnEndpointBeginInvoke(MessageEndpointActivatorBeginInvokeEventArgs endpointActivatorBeginInvokeEventArgs)
        {
            EventHandler<MessageEndpointActivatorBeginInvokeEventArgs> evt =
                this.MessageEndpointActivatorBeginInvoke;

            if (evt != null)
                evt(this, endpointActivatorBeginInvokeEventArgs);
        }

        ~MessageEndpointRegistry()
        {
            if (m_items != null)
            {
                foreach (var item in m_items)
                {
                    try
                    {
                        UnBindEndpointEvents(item);
                    }
                    catch
                    {
                        continue;
                    }
                }

                lock (m_lock)
                {
                    m_items.Clear();
                    m_items = null;
                }
            }
        }
    }
}