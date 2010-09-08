using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Internals.Reflection;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Aggregator.Impl
{
    /// <summary>
    /// Basic implementation of the aggregator EIP with custom implementations
    /// for aggregating and compiling the final result.
    /// </summary>
    /// <typeparam name="T">Type to aggregate over.</typeparam>
    public abstract class AbstractAggregatorMessageHandlingStrategy<T> : IAggregatorMessageHandlingStrategy<T>
    {
        private IObjectBuilder m_context = null;
        private string m_correlation_identifier = string.Empty;
        private List<T> m_accumulated_items = new List<T>();
        private AbstractAggregatorMessageHandlingStrategy<T> m_current_aggregator_message_handler_strategy;

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
        /// (Read-Only). Custom delegate strategy for determining when the aggregation strategy should complete. 
        /// </summary>
        public CompletionStrategyAction CompletionStrategy { get; private set; }

        /// <summary>
        /// (Read-Only). Custom delegate strategy for correlation messages together for aggregation.
        /// </summary>
        public CorrelationStrategyAction CorrelationStrategy { get; private set; }

        /// <summary>
        /// (Read-Only). This will contain the message that has been aggregated down from the collection of messages of similiar type.
        /// </summary>
        public IEnvelope AggregatedResult { get; private set; }

        /// <summary>
        /// (Read-Write). For native messages that can not be serialized, this is the property that contains the 
        /// array of objects for aggregation.
        /// </summary>
        public string PropertyContainingEnumeratedValues { get; set; }

        protected AbstractAggregatorMessageHandlingStrategy()
        {
        }

        /// <summary>
        /// This will set the instance of the concrete type that will 
        /// be performing the aggregator strategy.
        /// </summary>
        /// <param name="strategy"></param>
        public void SetStrategy(AbstractAggregatorMessageHandlingStrategy<T> strategy)
        {
            m_current_aggregator_message_handler_strategy = strategy;
        }

        /// <summary>
        /// This will set the corresponding context for the adapter to access any resources
        /// that it may need via the underlying object container.
        /// </summary>
        /// <param name="context"></param>
        public void SetContext(IObjectBuilder context)
        {
            m_context = context;
        }

        /// <summary>
        /// This will set the channel, by name, that the channel strategy will listen on for messages.
        /// </summary>
        /// <param name="channelName">Name of the input channel.</param>
        public void SetInputChannel(string channelName)
        {
            if (m_context != null)
            {
                var registry = m_context.Resolve<IChannelRegistry>();
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
            if (m_context != null)
            {
                var registry = m_context.Resolve<IChannelRegistry>();
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
        /// This will set the action to call to see if the aggregation 
        /// can complete.
        /// </summary>
        /// <param name="completionStrategy"></param>
        public void SetCompletionStrategy(CompletionStrategyAction completionStrategy)
        {
            CompletionStrategy = completionStrategy;
        }

        /// <summary>
        /// This will set the action to inspect to see if the messages are correlated.
        /// </summary>
        /// <param name="correlationStrategy"></param>
        public void SetCorrelationStrategy(CorrelationStrategyAction correlationStrategy)
        {
            CorrelationStrategy = correlationStrategy;
        }

        /// <summary>
        /// This will execute the custom strategy for the message on the channel.
        /// </summary>
        /// <param name="message"></param>
        public void ExecuteStrategy(IEnvelope message)
        {
            // by default, the strategy used to aggregate the 
            // messages is to set the sequence size to the number 
            // of items in the listing and correlate the results 
            // by the correlation id on the message. since all 
            // of this happens on the same thread, correlation
            // is implied but can be altered for individual use.

            object list = null;

            try
            {

                if (this.CurrentMethod != null)
                {
                    var attributes = CurrentMethod.GetCustomAttributes(typeof (AggregatorAttribute), true);

                    if (attributes.Length > 0)
                    {
                        var attr = (AggregatorAttribute) attributes[0];
                        if (!string.IsNullOrEmpty(attr.PropertyName))
                        {
                            var reflection = new DefaultReflection();
                            var instance = message.Body.GetPayload<object>();
                            list = reflection.GetPropertyValue(instance, attr.PropertyName);
                        }
                    }

                    if (!string.IsNullOrEmpty(this.PropertyContainingEnumeratedValues))
                    {
                        var reflection = new DefaultReflection();
                        var instance = message.Body.GetPayload<object>();
                        list = reflection.GetPropertyValue(instance, this.PropertyContainingEnumeratedValues);
                    }
                }

                if (list == null)
                    list = message.Body.GetPayload<object>();

                var sequence = 0;

                m_correlation_identifier = message.Header.CorrelationId;

                if (typeof(IEnumerable).IsAssignableFrom(list.GetType()))
                {
                    var iter = ((IEnumerable)list).GetEnumerator();
                    var item = message;
                    var sequenceSize = new ArrayList((ICollection)list).Count;

                    while (iter.MoveNext())
                    {
                        item.Body.SetPayload(iter.Current);
                        item.Header.SequenceNumber = sequence;
                        item.Header.SequenceSize = sequenceSize;

                        ++sequence;

                        if (IsComplete(item))
                            break;

                        if (!IsCorrelated(item))
                            break;

                        // do the aggregator strategy (i.e. select out the 
                        // messages that meet a criteria) here:
                        var output = DoAggregatorStrategy(item);

                        // accumulate the output:
                        if (output != null)
                            AccumulateOutput(output);
                    }

                    // do the aggregation strategy (i.e. combine the 
                    // set of aggregated messages into one message) here:
                    var result = this.DoAggregationStrategy(message);

                    if (result != null & !(result is NullEnvelope))
                        OnMessageAggregated(result);

                    if (InputChannel != null)
                        InputChannel.MessageSent -= InputChannel_Aggregator_MsgSent;
                }

            }
            catch (Exception exception)
            {
                var strategy = m_current_aggregator_message_handler_strategy != null
                                   ? m_current_aggregator_message_handler_strategy.GetType().FullName
                                   : "not specified";
                var msg =
                    string.Format(
                        "An error has occurred while implementing the custom aggregator strategy '{0}'. Reason: {1}",
                        strategy, exception.Message);
                throw new Exception(msg, exception);
            }
        }

        /// <summary>
        /// This will return the list of items that are set for aggregation.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<T> GetAccumulatedOutput()
        {
            return m_accumulated_items.AsReadOnly();
        }

        /// <summary>
        /// This will store the item from the list based on the selection criteria.
        /// </summary>
        /// <param name="item"></param>
        public void AccumulateOutput(T item)
        {
            m_accumulated_items.Add(item);
        }

        /// <summary>
        /// This will return the result of the aggregation.
        /// </summary>
        /// <returns></returns>
        public IEnvelope GetAggregatedResult()
        {
            return this.AggregatedResult;
        }

        /// <summary>
        /// This is the point in which each individual message
        /// will be checked to see if it can be stored for 
        /// creating the single message for subsequent processing.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract T DoAggregatorStrategy(IEnvelope message);

        /// <summary>
        /// This is the point where all of the messages that 
        /// have been accumulated based on the aggregator 
        /// strategy will be combined into one message payload
        /// for processing.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract IEnvelope DoAggregationStrategy(IEnvelope message);

        /// <summary>
        /// This will take the aggregated message and 
        /// send it to the output channel for processing.
        /// </summary>
        /// <param name="message"></param>
        public void OnMessageAggregated(IEnvelope message)
        {
            this.AggregatedResult = message;
            EventHandler<MessageHandlingStrategyCompleteEventArgs> evt = this.ChannelStrategyCompleted;

            // fire the event as we have an expected consumer:
            if (evt != null & OutputChannel != null)
                evt(this, new MessageHandlingStrategyCompleteEventArgs(OutputChannel.Name, message));

            if (evt == null & OutputChannel != null)
            {
                // clear the current destination so the 
                // output channel producer can deliver
                // to the configured location:
                message.Header.Destination = string.Empty;
                this.OutputChannel.Send(message);
            }
        }

        /// <summary>
        /// This will evaluate the message proccessing 
        /// against the barrier for completion.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool IsComplete(IEnvelope message)
        {
            var result = false;

            if (this.CompletionStrategy == null)
            {
                // default completion barrier:
                result = (message.Header.SequenceNumber == message.Header.SequenceSize);
            }
            else
            {
                result = this.CompletionStrategy.Invoke(message);
            }
            return result;
        }

        /// <summary>
        /// This will evaluate the message processing
        /// against the barrier for correlation or "same set".
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool IsCorrelated(IEnvelope message)
        {
            var result = false;

            if (this.CorrelationStrategy == null)
            {
                // default correlation barrier:
                result = (message.Header.CorrelationId == m_correlation_identifier);
            }
            else
            {
                result = this.CorrelationStrategy.Invoke(message);
            }

            return result;
        }

        /// <summary>
        /// This is the event-driven portion of the aggregator that will trigger 
        /// the aggregator and aggregation strateguies when a message is sent 
        /// on the input channel to the aggregator.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputChannel_Aggregator_MsgSent(object sender, ChannelMessageSentEventArgs e)
        {
            var messageToAggregate = e.Envelope;
            if (!(messageToAggregate is NullEnvelope))
                this.ExecuteStrategy(messageToAggregate);
        }

    }
}