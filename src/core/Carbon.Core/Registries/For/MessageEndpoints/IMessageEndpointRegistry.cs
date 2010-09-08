using System;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Subscription;
using Kharbon.Core;

namespace Carbon.Core.Registries.For.MessageEndpoints
{
    /// <summary>
    /// Contract for registering all message endpoints.
    /// </summary>
    public interface IMessageEndpointRegistry : IRegistry<IMessageEndpointActivator, Guid>
    {
        /// <summary>
        /// Event that is triggered when the component has started to invoke a method matching the message.
        /// </summary>
        event EventHandler<MessageEndpointActivatorBeginInvokeEventArgs> MessageEndpointActivatorBeginInvoke;

        /// <summary>
        /// Event that is triggered when the component has finished invoking a method matching the message.
        /// </summary>
        event EventHandler<MessageEndpointActivatorEndInvokeEventArgs> MessageEndpointActivatorEndInvoke;

        /// <summary>
        /// Event that is triggered when the component has generated an error invoking a method matching the message.
        /// </summary>
        event EventHandler<MessageEndpointActivatorErrorEventArgs> MessageEndpointActivatorError;

        /// <summary>
        /// This will configure a message end point from a subscription and register it 
        /// for future use in relaying messages to end points.
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        IMessageEndpointActivator ConfigureFromSubscription(ISubscription subscription);

        //void SetMessageBus(IMessageBus bus);

        /// <summary>
        /// This will activate an endpoint within the registry for processing a message via a subscription reference.
        /// </summary>
        /// <param name="subscription"><seealso cref="ISubscription"/>asubscription that defines how to route the message to the endpoint.</param>
        /// <param name="envelope"><seealso cref="IEnvelope"/>message to process.</param>
        /// <returns></returns>
        IEnvelope ActivateEndpointFromSubscription(ISubscription subscription, IEnvelope envelope);

        /// <summary>
        /// This will activate an endpoint within the registry for processing a message.
        /// </summary>
        /// <param name="activator"><seealso cref="IMessageEndpointActivator"/>activator that will process the message</param>
        /// <param name="envelope"><seealso cref="IEnvelope"/>message to process.</param>
        /// <returns></returns>
        IEnvelope ActivateEndpoint(IMessageEndpointActivator activator, IEnvelope envelope);

        /// <summary>
        /// This will create a messaging endpoint from the information and register it for invocation.
        /// </summary>
        /// <param name="inputChannel">Name of the channel where the message will be received from.</param>
        /// <param name="outputChannel">Name of the channel where the message will be sent to (optional)</param>
        /// <param name="methodName">Name of the method on the endpoint(component) that will process the message (optional)</param>
        /// <param name="endpoint">Instance of the endpoint that will handle the message.</param>
        void CreateEndpoint(string inputChannel, string outputChannel, string methodName, object endpoint);
    }
}