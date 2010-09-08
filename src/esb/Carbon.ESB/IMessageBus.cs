using System;
using Carbon.Core;
using Carbon.Core.RuntimeServices;
using Carbon.ESB.Saga;
using Carbon.Core.Adapter;
using Carbon.ESB.Configuration;

namespace Carbon.ESB
{
    /// <summary>
    /// Contract for conversations to mediate the publish-subscribe mode of messaging 
    /// for long running tasks across messaging endpoints.
    /// </summary>
    public interface IMessageBus : IStartable
    {
        /// <summary>
        /// Event that is triggered when a message is delivered to a messaging endpoint.
        /// </summary>
        event EventHandler<MessageBusMessageDeliveredEventArgs> MessageBusMessageDelivered;

        /// <summary>
        /// Event that is triggered when a message is received from a messaging endpoint.
        /// </summary>
        event EventHandler<MessageBusMessageReceivedEventArgs> MessageBusMessageReceived;

        /// <summary>
        /// Event that is triggered when a message endpoint encounters an error.
        /// </summary>
        event EventHandler<MessageBusErrorEventArgs> MessageBusError;

        /// <summary>
        /// Event that is triggered when a message is published to an endpoint.
        /// </summary>
        event EventHandler<MessageBusMessagePublishedEventArgs> MessagePublished;

        /// <summary>
        /// (Read-Write). The channel adapter that will represent the bus listening for messages and writing 
        /// them to a local channel for processing out to message endpoints.
        /// </summary>
        AbstractInputChannelAdapter Endpoint { get; set; }

        /// <summary>
        /// (Read-Write). The logical channel where the bus is located.
        /// </summary>
        string LocalChannel { get; set; }

        /// <summary>
        /// (Read-Write). The address where the bus is located.
        /// </summary>
        string LocalAddress { get; set; }

        /// <summary>
        /// (Read-Write). The address where all remote subscriptions that can not be resolved locally are sent 
        /// for resolution for publication.
        /// </summary>
        string SubscriptionAddress { get; set; }

        /// <summary>
        /// (Read-Write). The value set from the configuration to determine whether or not the end points 
        /// will be configured from their code definition (i.e. attributes) or the configuration file for messaging.
        /// </summary>
        bool IsAnnotationDriven { get; set; }

        /// <summary>
        /// This will allow for retrieval of a dependant component for a
        /// messaging endpoint conversation.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component to retrieve from the component container.</typeparam>
        /// <returns></returns>
        TComponent GetComponent<TComponent>();

        /// <summary>
        /// This will allow for construction of a dependant component for a
        /// messaging endpoint conversation.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component to create from the existing set of conversation messages.</typeparam>
        TComponent CreateComponent<TComponent>() where TComponent : ISagaMessage;

        /// <summary>
        /// This will take a message that is on the persistant storage location for the end point
        /// and dispatch it to the component for processing.
        /// </summary>
        /// <param name="message"></param>
        void Publish(IEnvelope message);

        /// <summary>
        /// This will send a message out to the set of messaging endpoints that can process the message.
        /// </summary>
        /// <param name="messages">Message(s) to send</param>
        void Publish(params object[] messages);

        /// <summary>
        /// This will send a message out to the set of messaging endpoints for an existing conversation 
        /// that can process the message, correlating the message to the current conversation.
        /// </summary>
        /// <typeparam name="TMessage">Type of the message to send</typeparam>
        /// <param name="saga">The existing conversation that will publish out a message.</param>
        /// <param name="message">Message to send</param>
        void Publish<TMessage>(ISaga saga, params TMessage[] message)
            where TMessage : class, ISagaMessage;

        /// <summary>
        /// This will send a message indicating that the initial message
        /// that is queued for delayed delivery should be cancelled 
        /// for the existing conversation, correlating the message to the current conversation.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to look for for cancellation.</typeparam>
        /// <param name="saga">The existing conversation that will send a cancellation for a previously published message.</param>
        void CancelPublication<TMessage>(ISaga saga)
            where TMessage : ISagaMessage, new();

        /// <summary>
        /// This will delay the publication of a message to the set of messaging endponits
        /// that can process the message, correlating the message to the current conversation.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to send</typeparam>
        /// <param name="saga">The existing conversation that will delay a message for publication.</param>
        /// <param name="waitInterval"><see cref="TimeSpan"/>Timespan to wait before delivery</param>
        /// <param name="message">Message to deliver</param>
        void DelayPublication<TMessage>(ISaga saga, TimeSpan waitInterval, TMessage message)
            where TMessage : class, ISagaMessage;

        /// <summary>
        /// This will allow an individual messaging endpoint to be manually configured 
        /// or "bootstrapped" into the message bus for receiving/sending messages.
        /// </summary>
        /// <typeparam name="TEndpointConfiguration"></typeparam>
        void Configure<TEndpointConfiguration>()
            where TEndpointConfiguration : AbstractBootStrapper, new();
    }
}