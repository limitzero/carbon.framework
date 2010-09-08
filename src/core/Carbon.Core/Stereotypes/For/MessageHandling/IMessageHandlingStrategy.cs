using System;
using System.Reflection;
using Carbon.Core.Channel;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Builder;

namespace Carbon.Core.Stereotypes.For.MessageHandling
{
    /// <summary>
    /// Contract that is used for the component that has the <seealso cref="MessageEndpointAttribute">message endpoint</seealso>
    /// annotation to change the behavior of how the message is handled once it reaches the method on the component for processing.
    /// </summary>
    public interface IMessageHandlingStrategy
    {
        /// <summary>
        /// Event that is triggered when the strategy has been completed.
        /// </summary>
        event EventHandler<MessageHandlingStrategyCompleteEventArgs> ChannelStrategyCompleted;

        /// <summary>
        /// (Read-Only). The channel that the source message will be produced on for 
        /// compiling into one message for delivery to the output channel.
        /// </summary>
        AbstractChannel InputChannel { get; }

        /// <summary>
        /// (Read-Only). The channel that the reconstructed message will be produced 
        /// for subsequent processing.
        /// </summary>
        AbstractChannel OutputChannel { get; }

        /// <summary>
        /// (Read-Only). The current method that is invoked to implement the channel strategy.
        /// </summary>
        MethodInfo CurrentMethod { get; }

        /// <summary>
        /// (Read-Only). The current object instance where the method is being invoked for the channel strategy.
        /// </summary>
        object CurrentInstance { get; }

        /// <summary>
        /// This will set the corresponding context for the adapter to access any resources
        /// that it may need via the underlying object container.
        /// </summary>
        /// <param name="objectBuilder"></param>
        void SetContext(IObjectBuilder objectBuilder);

        /// <summary>
        /// This will set the channel, by name, that the channel strategy will listen on for messages.
        /// </summary>
        /// <param name="channelName">Name of the input channel.</param>
        void SetInputChannel(string channelName);

        /// <summary>
        /// This will set the channel that the channel strategy will listen on for messages.
        /// </summary>
        /// <param name="channel">Input channel for individual source message.</param>
        void SetInputChannel(AbstractChannel channel);

        /// <summary>
        /// This will set the channel, by name, that the channel strategy will deliver the message to after processing.
        /// </summary>
        /// <param name="channelName">Name of the output channel.</param>
        void SetOutputChannel(string channelName);

        /// <summary>
        /// This will set the channel that the channel strategy will deliver the message to after processing.
        /// </summary>
        /// <param name="channel">Output channel for the individual composed message.</param>
        void SetOutputChannel(AbstractChannel channel);

        /// <summary>
        /// This will set the value of the current instance where the method that is implementing the channel strategy is located.
        /// </summary>
        /// <param name="instance"></param>
        void SetInstance(object instance);

        /// <summary>
        /// This will set the value of the current method that is implementing the channel strategy.
        /// </summary>
        /// <param name="method"></param>
        void SetMethod(MethodInfo method);

        /// <summary>
        /// This will set the value of the current method that is implementing the channel strategy.
        /// </summary>
        /// <param name="name">Name of the method on the instance that will be executing the strategy.</param>
        void SetMethod(string name);

        /// <summary>
        /// This will execute the custom strategy for the message on the channel.
        /// </summary>
        /// <param name="message"></param>
        void ExecuteStrategy(IEnvelope message);
    }
}