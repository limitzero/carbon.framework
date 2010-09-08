using System;
using Carbon.Core.Adapter;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Stereotypes.For.Components.Service.Impl;
using Carbon.Core.RuntimeServices;

namespace Carbon.Integration
{
    /// <summary>
    /// Contract for registering all components that will participate in an application integration context
    /// for linear messaging exchanges.
    /// </summary>
    public interface IApplicationContext : IStartable
    {
        /// <summary>
        /// Event that is triggered when a message is delivered to a messaging endpoint.
        /// </summary>
        event EventHandler<ApplicationContextMessageDeliveredEventArgs> ApplicationContextMessageDelivered;

        /// <summary>
        /// Event that is triggered when a message is received from a messaging endpoint.
        /// </summary>
        event EventHandler<ApplicationContextMessageReceivedEventArgs> ApplicationContextMessageReceived;

        /// <summary>
        /// Event that is triggered when a message endpoint encounters an error.
        /// </summary>
        event EventHandler<ApplicationContextErrorEventArgs> ApplicationContextError;

        void RegisterServiceEndpointActivator(IServiceActivator serviceActivator);

        void RegisterMessageEndpointActivator(IMessageEndpointActivator messageEndpointActivator);

        void RegisterInputChannelAdapter(AbstractInputChannelAdapter adapter); 

        void RegisterOutputChannelAdapter(AbstractOutputChannelAdapter adapter);

        TComponent GetComponent<TComponent>();
    }
}