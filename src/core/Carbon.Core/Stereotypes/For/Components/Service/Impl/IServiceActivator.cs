using System;
using Carbon.Core.Channel;

namespace Carbon.Core.Stereotypes.For.Components.Service.Impl
{
    public interface IServiceActivator
    {
        /// <summary>
        /// Event that is triggered when the component has started to invoke a method matching the message.
        /// </summary>
        event EventHandler<ServiceActivatorBeginInvokeEventArgs> ServiceActivatorBeginInvoke;

        /// <summary>
        /// Event that is triggered when the component has finished invoking a method matching the message.
        /// </summary>
        event EventHandler<ServiceActivatorEndInvokeEventArgs> ServiceActivatorEndInvoke;

        /// <summary>
        /// Event that is triggered when the component has generated an error invoking a method matching the message.
        /// </summary>
        event EventHandler<ServiceActivatorErrorEventArgs> ServiceActivatorError;

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
        object ServiceInstance { get; }

        /// <summary>
        /// (Read-Only). The name of the method that has been either set or resolved internally for processing the message on the 
        /// service instance.
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// This will set the input channel for the service activated component for receiving a message.
        /// </summary>
        /// <param name="name">The name of the channel</param>
        void SetInputChannel(string name);

        /// <summary>
        /// This will set the input channel for the service activated component for receiving a message.
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
        /// <param name="serviceInstance"></param>
        void SetServiceInstance(object serviceInstance);

        /// <summary>
        /// This will set the method that should be invoked for the message on the component 
        /// if known before invocation.
        /// </summary>
        /// <param name="name"></param>
        void SetServiceInstanceMethodName(string name);
    }
}