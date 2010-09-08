using Carbon.Channel.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Channel.Impl.Queue;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Subscription;
using Carbon.ESB.Services;

namespace Carbon.ESB.Registries.Endpoints
{
    public class ServiceBusEndpointRegistry : MessageEndpointRegistry, IServiceBusEndpointRegistry
    {
        private IObjectBuilder m_builder = null;
        private IMessageBus m_message_bus = null;

        public ServiceBusEndpointRegistry(IObjectBuilder container)
            : base(container)
        {
            m_builder = container;
        }

        public void SetMessageBus(IMessageBus bus)
        {
            m_message_bus = bus;
        }

        public override IMessageEndpointActivator ConfigureFromSubscription(ISubscription subscription)
        {
            IMessageEndpointActivator activator = null;

            // first look to see if it has already been configured
            foreach (var endpointActivator in GetAllItems())
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
            var type = m_builder.Resolve<IReflection>().FindTypeForName(subscription.Component);
            var instance = m_builder.Resolve(type);

            if (instance == null)
                return activator;

            activator = m_builder.Resolve<IMessageEndpointActivator>();

            if (typeof(IMessageBusService).IsAssignableFrom(instance.GetType()))
                ((IMessageBusService)instance).Bus = this.m_message_bus;

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
    }
}