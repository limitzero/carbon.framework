using System;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Subscription;
using Carbon.Core.Registries.For.MessageEndpoints;

namespace Carbon.ESB.Registries.Endpoints
{
    public interface IServiceBusEndpointRegistry : IMessageEndpointRegistry
    {
        /// <summary>
        /// This will assign the local bus instance to the message endpoint activator.
        /// </summary>
        /// <param name="bus"></param>
        void SetMessageBus(IMessageBus bus);

        /// <summary>
        /// This will create a message endpoint activator from the subscription definition.
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        IMessageEndpointActivator ConfigureFromSubscription(ISubscription subscription);

    }
}