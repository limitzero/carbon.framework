using System;
using System.Reflection;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Internals.MessageResolution;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Stereotypes.For.MessageHandling;

namespace Carbon.Integration.Stereotypes.Consumes.Impl
{
    /// <summary>
    /// Concrete instance for handling the <seealso cref="ConsumesAttribute">consumes</seealso>
    /// annotation on a method for a <seealso cref="MessageEndpointAttribute">message endpoint</seealso>.
    /// </summary>
    public class ConsumesMessageHandlingStrategy : IConsumesMessageHandlingStrategy
    {
        private IObjectBuilder m_object_builder = null;

        /// <summary>
        /// Event that is triggered when the strategy has been completed.
        /// </summary>
        public event EventHandler<MessageHandlingStrategyCompleteEventArgs> ChannelStrategyCompleted;

        /// <summary>
        /// (Read-Only). The channel that the source message will be produced on for 
        /// compiling into one message for delivery to the output channel.
        /// </summary>
        public AbstractChannel InputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The channel that the reconstructed message will be produced 
        /// for subsequent processing.
        /// </summary>
        public AbstractChannel OutputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The current method that is invoked to implement the channel strategy.
        /// </summary>
        public MethodInfo CurrentMethod { get; private set; }

        /// <summary>
        /// (Read-Only). The current object instance where the method is being invoked for the channel strategy.
        /// </summary>
        public object CurrentInstance { get; private set; }

        /// <summary>
        /// This will set the corresponding context for the adapter to access any resources
        /// that it may need via the underlying object container.
        /// </summary>
        /// <param name="objectBuilder"></param>
        public void SetContext(IObjectBuilder objectBuilder)
        {
            m_object_builder = objectBuilder;
        }

        /// <summary>
        /// This will set the channel, by name, that the channel strategy will listen on for messages.
        /// </summary>
        /// <param name="channelName">Name of the input channel.</param>
        public void SetInputChannel(string channelName)
        {
            if (m_object_builder != null)
            {
                var registry = m_object_builder.Resolve<IChannelRegistry>();
                if (registry != null)
                {
                    var channel = registry.FindChannel(channelName);

                    if (!(channel is NullChannel))
                        SetInputChannel(channel);
                }
            }
        }

        /// <summary>
        /// This will set the channel that the channel strategy will listen on for messages.
        /// </summary>
        /// <param name="channel">Input channel for individual source message.</param>
        public void SetInputChannel(AbstractChannel channel)
        {
            InputChannel = channel;
            InputChannel.MessageSent += InputChannel_Aggregator_MsgSent;
        }

        /// <summary>
        /// This will set the channel, by name, that the channel strategy will deliver the message to after processing.
        /// </summary>
        /// <param name="channelName">Name of the output channel.</param>
        public void SetOutputChannel(string channelName)
        {
            if (m_object_builder != null)
            {
                var registry = m_object_builder.Resolve<IChannelRegistry>();
                if (registry != null)
                {
                    var channel = registry.FindChannel(channelName);

                    if (!(channel is NullChannel))
                        SetOutputChannel(channel);
                }
            }
        }

        /// <summary>
        /// This will set the channel that the channel strategy will deliver the message to after processing.
        /// </summary>
        /// <param name="channel">Output channel for the individual composed message.</param>
        public void SetOutputChannel(AbstractChannel channel)
        {
            OutputChannel = channel;
        }

        /// <summary>
        /// This will set the value of the current instance where the method that is implementing the channel strategy is located.
        /// </summary>
        /// <param name="instance"></param>
        public void SetInstance(object instance)
        {
            CurrentInstance = instance;
        }

        /// <summary>
        /// This will set the value of the current method that is implementing the channel strategy.
        /// </summary>
        /// <param name="method"></param>
        public void SetMethod(MethodInfo method)
        {
            CurrentMethod = method;
        }

        /// <summary>
        /// This will set the value of the current method that is implementing the channel strategy.
        /// </summary>
        /// <param name="name">Name of the method on the instance that will be executing the strategy.</param>
        public void SetMethod(string name)
        {
            if (this.CurrentInstance != null)
                this.CurrentMethod = this.CurrentInstance.GetType().GetMethod(name);
        }

        /// <summary>
        /// This will execute the custom strategy for the message on the channel.
        /// </summary>
        /// <param name="message"></param>
        public void ExecuteStrategy(IEnvelope message)
        {
            var destination = string.Empty;

            try
            {

                if (CurrentMethod == null)
                {
                    var mapper = new MapMessageToMethod();
                    var method = mapper.Map(this.CurrentInstance, message);
                    this.CurrentMethod = method;
                }

                if (CurrentMethod != null)
                {
                    var attributes = CurrentMethod.GetCustomAttributes(typeof(ConsumesAttribute), true);

                    if (attributes.Length > 0)
                    {
                        // find the new output channel (this is the core strategy of the "Consumes" attribute):
                        destination = ((ConsumesAttribute)attributes[0]).OutputChannel;

                        if (!string.IsNullOrEmpty(destination))
                            SetOutputChannel(destination);
                    }

                    // invoke the method:
                    var invoker = new MappedMessageToMethodInvoker(this.CurrentInstance, this.CurrentMethod);
                    message = invoker.Invoke(message);

                    // re-direct the message:
                    OnConsumerStrategyCompleted(destination, message);

                }
            }
            catch (ThreadAbortException threadAbortException)
            {
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                if (this.InputChannel != null && this.InputChannel is NullChannel)
                    this.InputChannel.MessageSent -= InputChannel_Aggregator_MsgSent;
            }

        }

        private void OnConsumerStrategyCompleted(string channel, IEnvelope message)
        {
            // re-direct the message:
            if (!(message is NullEnvelope))
            {
                EventHandler<MessageHandlingStrategyCompleteEventArgs> evt = this.ChannelStrategyCompleted;

                if (evt != null)
                    evt(this, new MessageHandlingStrategyCompleteEventArgs(channel, message));
            }
        }

        private void InputChannel_Aggregator_MsgSent(object sender, ChannelMessageSentEventArgs e)
        {
            var message = e.Envelope;
            if (!(message is NullEnvelope))
                this.ExecuteStrategy(message);
        }
    }
}