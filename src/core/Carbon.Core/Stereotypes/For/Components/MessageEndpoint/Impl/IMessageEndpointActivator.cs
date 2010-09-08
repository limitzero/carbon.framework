using System;
using Carbon.Core.Channel;
using Kharbon.Core;

namespace Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl
{
    /// <summary>
    /// This will determine how the messaging end point will be invoked to process a message.
    /// </summary>
    public enum EndpointActivationStyle
    {
        /// <summary>
        /// This will trigger the message endpoint to process the message as soon 
        /// as it is sent by caller (integration-style messaging)
        /// </summary>
        ActivateOnMessageSent,

        /// <summary>
        /// This will trigger the message endpoint to process the message when it 
        /// has been received from the persistant storage area (message bus style messaging).
        /// </summary>
        ActivateOnMessageReceived,

        ActivateForBiDirectional
    }

    public interface IMessageEndpointActivator
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
        /// (Read-Only). The instance identifier of the endpoint activator.
        /// </summary>
        Guid Id { get;  }

        /// <summary>
        /// (Read-Write). This will determine how the messaging end point will be invoked to process a message.
        /// Operates in <seealso cref="EndpointActivationStyle.ActivateOnMessageSent"/> mode by default.
        /// </summary>
        EndpointActivationStyle ActivationStyle { get; set; }

        /// <summary>
        /// (Read-Only). The channel by which the message will be delivered to the component for processing.
        /// </summary>
        AbstractChannel InputChannel { get; }

        /// <summary>
        /// (Read-Only). The channel by which the processed message will be delivered.
        /// </summary>
        AbstractChannel OutputChannel { get; }

        /// <summary>
        /// (Read-Only). The instance of the object that will be used to process the message over the input and output channels.
        /// </summary>
        object EndpointInstance { get; }

        /// <summary>
        /// (Read-Only). The name of the method that has been either set or resolved internally for processing the message on the 
        /// service instance.
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// (Read-Only). The message that is returned from the result of calling the end point.
        /// </summary>
        IEnvelope ReturnMessage { get; }

        /// <summary>
        /// (Read-Only). The type of the instance that will be used to process the message over the input and output channels.
        /// </summary>
        Type EndpointInstanceType { get; }

        /// <summary>
        /// This will set the input channel for the service activated component for receiving a message.
        /// </summary>
        /// <param name="name">The name of the channel</param>
        void SetInputChannel(string name);

        /// <summary>
        /// This will set the input channel for the message activated component for receiving a message.
        /// </summary>
        /// <param name="channel">The channel that will hold the contents for the message to be processed.</param>
        void SetInputChannel(AbstractChannel channel);

        /// <summary>
        /// This will set the output channel where the message will be delivered after processing.
        /// </summary>
        /// <param name="name"></param>
        void SetOutputChannel(string name);

        /// <summary>
        /// This will set the output channel where the message will be delivered after processing.
        /// </summary>
        /// <param name="channel">The channel that will hold the contents of the processed message.</param>
        void SetOutputChannel(AbstractChannel channel);

        /// <summary>
        /// This will set the activated instance of the component for processing the  
        /// message over the input and output channels.
        /// </summary>
        /// <param name="endpointInstance"></param>
        void SetEndpointInstance(object endpointInstance);

        /// <summary>
        /// This will set the method that should be invoked for the message on the component 
        /// if known before invocation.
        /// </summary>
        /// <param name="name"></param>
        void SetEndpointInstanceMethodName(string name);

        /// <summary>
        /// This will invoke the endpoint and apply any message handling behavior.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IEnvelope InvokeEndpoint(IEnvelope message);

        void SetEndpointInstance(Type endpoint);
    }
}