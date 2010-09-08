using System;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Internals.MessageResolution;
using Carbon.Core.Stereotypes.For.MessageHandling;

namespace Carbon.Integration.Stereotypes.Splitter.Impl
{
    public abstract class AbstractSplitterMessageHandlingStrategy : ISplitterMessageHandlingStrategy
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
        ///  (Read-Write). Flag to indicate whether or not the output channel can have duplicates (default = false)
        /// </summary>
        public bool IsDuplicatesAllowedOnOutputChannel
        {
            get; set;
        }

        protected AbstractSplitterMessageHandlingStrategy()
        {
            IsDuplicatesAllowedOnOutputChannel = false;
        }

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
            InputChannel.MessageSent += InputChannelMessageSent;
            InputChannel.MessageReceived += InputChannelMessageReceived;
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
            try
            {
                var outputChannelName = string.Empty;
                var destination = string.Empty;

                try
                {
                    if (CurrentMethod == null)
                    {
                        var mapper = new MapMessageToMethod();
                        var method = mapper.Map(this.CurrentInstance, message);
                        this.CurrentMethod = method;
                    }  
                }
                catch (Exception exception)
                {
                    string msg =
                        string.Format(
                            "The component {0} does not have the method {1} defined for accepting the message for splitting.",
                            this.CurrentInstance.GetType().FullName,
                            this.CurrentMethod.Name);
                    throw new Exception(msg, exception);
                }

                if (this.CurrentMethod != null)
                {
                    var attributes = this.CurrentMethod.GetCustomAttributes(typeof(SplitterAttribute), true);

                    if (attributes.Length > 0)
                        outputChannelName = ((SplitterAttribute)attributes[0]).OutputChannelName;
                }
                else
                {
                    // need to find the first method with the Splitter attribute:
                    foreach (var method in this.CurrentInstance.GetType().GetMethods())
                    {
                        var attributes = method.GetCustomAttributes(typeof(SplitterAttribute), true);

                        if (method.GetCustomAttributes(typeof(SplitterAttribute), true).Length > 0)
                        {
                            this.CurrentMethod = method;

                            outputChannelName = ((SplitterAttribute)attributes[0]).OutputChannelName;

                            break;
                        }
                    }

                }

                // set the channel on which to send the individual messages:
                if (!string.IsNullOrEmpty(outputChannelName))
                    this.SetOutputChannel(outputChannelName);

                if (!(this.OutputChannel is NullChannel))
                {
                    // execute the method and get the results to send to the output channel 
                    // (we split the message payload, not the passed in message object!!!):
                    var invoker = new MappedMessageToMethodInvoker(this.CurrentInstance, this.CurrentMethod);
                    message = invoker.Invoke(message.Body.GetPayload<object>());

                    // signal the behavior of the output channel:
                    this.OutputChannel.IsIdempotent = IsDuplicatesAllowedOnOutputChannel;

                    // split (i.e. deliver the individual messages) the resultant messages to the output channel:
                    this.DoSplitterStrategy(message);
                }

            }
            catch (Exception exception)
            {
                var msg =
                    string.Format(
                        "An error has occurred while attempting the splitter strategy for component {0} and method {1}. Reason: {2}",
                        this.CurrentInstance.GetType().FullName,
                        this.CurrentMethod.Name,
                        exception.Message);
                throw new Exception(msg, exception);
            }
        }

        /// <summary>
        /// This is the custom part of the splitter strategy where the message
        /// is split according to user-defined logic. When the message has been 
        /// split into a smaller part, the <see cref="OnMessageSplit"/> method
        /// should be called to forward the de-composed message part to the 
        /// configured output channel.
        /// </summary>
        /// <param name="message">Message to split.</param>
        public abstract void DoSplitterStrategy(IEnvelope message);

        /// <summary>
        /// This will send the split message to the output channel for processing.
        /// </summary>
        /// <param name="message">Message that is split.</param>
        public void OnMessageSplit(IEnvelope message)
        {
            var nextChannel = string.Empty;
            EventHandler<MessageHandlingStrategyCompleteEventArgs> evt = this.ChannelStrategyCompleted;

            if (!(this.OutputChannel is NullChannel))
                nextChannel = this.OutputChannel.Name;

            // send to the event-driven consumer for this event:
            if (evt != null)
                evt(this, new MessageHandlingStrategyCompleteEventArgs(nextChannel, message));

            // push the message onto the channel:
            if (!string.IsNullOrEmpty(nextChannel))
                OutputChannel.Send(message);
        }

        private void InputChannelMessageSent(object sender, ChannelMessageSentEventArgs e)
        {
            var messageToSplit = e.Envelope;

            if (!(messageToSplit is NullEnvelope))
                if (messageToSplit.Body.Payload != null)
                    this.ExecuteStrategy(messageToSplit);
        }

        private void InputChannelMessageReceived(object sender, ChannelMessageReceivedEventArgs e)
        {
            var messageToSplit = e.Envelope;

            if (!(messageToSplit is NullEnvelope))
                if (messageToSplit.Body.Payload != null)
                    this.ExecuteStrategy(messageToSplit);
        }

    }
}
