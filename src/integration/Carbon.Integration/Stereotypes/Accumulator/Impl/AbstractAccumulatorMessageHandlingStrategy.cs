using System;
using System.Collections.Generic;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Stereotypes.For.MessageHandling;
using System.Xml.Serialization;
using Carbon.Core.Internals.MessageResolution;
using System.Timers;

namespace Carbon.Integration.Stereotypes.Accumulator.Impl
{
    /// <summary>
    /// Basic implementation of the accumulator EIP with custom implementations for signaling the completion.
    /// State is preserved via a simple IList{T} implementation (which is a static instance that is provided via the 
    /// client accumulation strategy)
    /// </summary>
    /// <typeparam name="T">Type to accumulate over.</typeparam>
    public abstract class AbstractAccumulatorMessageHandlingStrategy<T> : IAccumulatorMessageHandlingStrategy<T>
    {
        private static int _current_batch_size = 0;
        private static DateTime _start_marker = DateTime.MinValue;
        private static DateTime _end_marker = DateTime.MinValue;
        private static TimeSpan? _interval_for_collection = null;
        private bool _send_as_batch = false;
        private IObjectBuilder _object_builder = null;

        public const int BATCH_SIZE_LIMIT = 256;

        /// <summary>
        /// Event that is triggered when the strategy has been completed.
        /// </summary>
        public event EventHandler<MessageHandlingStrategyCompleteEventArgs> ChannelStrategyCompleted;

        /// <summary>
        /// (Read-Only). The channel that the source message will be produced on for 
        /// compiling into one message for delivery to the output channel.
        /// </summary>
        [XmlIgnore]
        public AbstractChannel InputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The channel that the reconstructed message will be produced 
        /// for subsequent processing.
        /// </summary>
        [XmlIgnore]
        public AbstractChannel OutputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The current method that is invoked to implement the channel strategy.
        /// </summary>
        [XmlIgnore]
        public MethodInfo CurrentMethod { get; private set; }

        /// <summary>
        /// (Read-Only). The current object instance where the method is being invoked for the channel strategy.
        /// </summary>
        [XmlIgnore]
        public object CurrentInstance { get; private set; }

        [XmlIgnore]
        public Action CleanUpAction { get; set; }

        /// <summary>
        /// (Read-Only). The listing of accumulated items.
        /// </summary>
        public IList<T> AccumulatedItemsStorage
        {
            get;
            private set;
        }

        /// <summary>
        /// (Read-Write). Flag to indicate whether or not to keep duplicate messages in the accumulated output.
        /// </summary>
        public bool EnsureUniqueItems { get; set; }

        /// <summary>
        /// (Read-Write). Number of messages to accumulate at one instance. Maximum is 256.
        /// </summary>
        private int _message_batch_size = BATCH_SIZE_LIMIT;

        private bool _ensure_unique_items = false;

        public int MessageBatchSize
        {
            get
            {
                return _message_batch_size;
            }

            set
            {
                if (value > BATCH_SIZE_LIMIT)
                    throw new ArgumentException("The message batch size can not exceed " + BATCH_SIZE_LIMIT.ToString() + ". Please choose a batch size less than or equal to the upper limit.");
                _message_batch_size = value;
            }
        }

        protected AbstractAccumulatorMessageHandlingStrategy()
        {
            AccumulatedItemsStorage = new List<T>();
        }

        /// <summary>
        /// This will set the corresponding context for the adapter to access any resources
        /// that it may need via the underlying object container.
        /// </summary>
        /// <param name="objectBuilder"></param>
        public void SetContext(IObjectBuilder objectBuilder)
        {
            _object_builder = objectBuilder;
        }

        /// <summary>
        /// This will set the channel, by name, that the channel strategy will listen on for messages.
        /// </summary>
        /// <param name="channelName">Name of the input channel.</param>
        public void SetInputChannel(string channelName)
        {
            if (_object_builder != null)
            {
                var registry = _object_builder.Resolve<IChannelRegistry>();
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
            if (_object_builder != null)
            {
                var registry = _object_builder.Resolve<IChannelRegistry>();
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
        /// This will allow the component that is maintaining the 
        /// storage of the accumulated items to pass in the persistant
        /// storage list for the strategy to add items to.
        /// </summary>
        /// <param name="storage"></param>
        public void SetStorage(IList<T> storage)
        {
            this.AccumulatedItemsStorage = storage;
        }

        /// <summary>
        /// This will execute the custom strategy for the message on the channel.
        /// </summary>
        /// <param name="message"></param>
        public void ExecuteStrategy(IEnvelope message)
        {
            try
            {
                if (CurrentMethod == null)
                {
                    var mapper = new MapMessageToMethod();
                    var method = mapper.Map(this.CurrentInstance, message);
                    this.CurrentMethod = method;
                }

                InitializeStrategy();

                _current_batch_size++;

                // first check the completion criteria and the message batch size to see if either has been satisfied:
                var item = message.Body.GetPayload<T>();

                if (_start_marker == DateTime.MinValue)
                {
                    if ((_current_batch_size != this.MessageBatchSize) &&
                        !IsComplete(message, item, this.AccumulatedItemsStorage.Count) &&
                        (_current_batch_size <= BATCH_SIZE_LIMIT))
                    {
                        // send the next message to the accumulator for storage:
                        SendToLocalStorage(message);
                    }
                    else
                    {
                        // add the last message !!!
                        SendToLocalStorage(message);

                        // signal that the strategy has been completed:
                        DispatchMessagesToChannel();
                    }
                }
                else
                {
                    // append the message to the storage for the duration...
                    if (DateTime.Now <= _end_marker)
                        SendToLocalStorage(message);
                    else
                    {
                        // clean-up the markers:
                        _start_marker = DateTime.MinValue;
                        _end_marker = _start_marker;

                        // signal that the strategy has been completed:
                        DispatchMessagesToChannel();
                    }
                }

            }
            catch (Exception exception)
            {
                var msg =
                     string.Format(
                         "An error has occurred while attempting the accumulator strategy for component {0} and method {1}. Reason: {2}",
                         this.CurrentInstance.GetType().FullName,
                         this.CurrentMethod.Name,
                         exception.Message);
                throw new Exception(msg, exception);
            }
            finally
            {
                if (this.InputChannel != null && this.InputChannel is NullChannel)
                    this.InputChannel.MessageSent -= InputChannel_Aggregator_MsgSent;
            }
        }

        /// <summary>
        /// (User-defined). This is the completion strategy for the accumulator
        /// </summary>
        /// <param name="message"></param>
        /// <param name="item">Current payload to inspect</param>
        /// <param name="numberOfItemsInStorage">Current number of items accumulated.</param>
        /// <returns></returns>
        public virtual bool IsComplete(IEnvelope message, T item, int numberOfItemsInStorage)
        {
            return false;
        }

        private void InitializeStrategy()
        {
            AccumulateAttribute accumulateAttribute = null;
            AccumulatorAttribute accumulatorAttribute = null;
            var attributes = new object[] { };
            var destination = string.Empty;

            // accumulate over simple message or use custom strategy for accumulator?
            this.MessageBatchSize = BATCH_SIZE_LIMIT;

            attributes = CurrentMethod.GetCustomAttributes(typeof(AccumulatorAttribute), true);
            if (attributes.Length > 0)
            {
                accumulatorAttribute = attributes[0] as AccumulatorAttribute;
            }

            attributes = CurrentMethod.GetCustomAttributes(typeof(AccumulateAttribute), true);
            if (attributes.Length > 0)
            {
                accumulateAttribute = attributes[0] as AccumulateAttribute;
            }

            if (accumulatorAttribute != null)
            {
                _send_as_batch = accumulatorAttribute.SendResultAsBatch;
                destination = accumulatorAttribute.ChannelToSendListing;
                _interval_for_collection = CreateTimespanFromInterval(accumulatorAttribute.IntervalForCollection);
                MessageBatchSize = accumulatorAttribute.BatchSize > 0 ? accumulatorAttribute.BatchSize : MessageBatchSize;
            }

            if (accumulateAttribute != null)
            {
                _send_as_batch = accumulateAttribute.SendResultAsBatch;
                _interval_for_collection = CreateTimespanFromInterval(accumulateAttribute.IntervalForCollection);
                _ensure_unique_items = accumulateAttribute.EnsureUniqueItems;
                MessageBatchSize = accumulateAttribute.BatchSize > 0 ? accumulateAttribute.BatchSize : MessageBatchSize;
            }

            if (string.IsNullOrEmpty(destination) && this.OutputChannel is NullChannel)
                throw new ArgumentException("For the accumulator strategy to deliver the set of compiled messages, " +
                    "a destination channel name must be defined for the accumulator attribute attached to the method '" +
               CurrentMethod.Name + "' on the component '" + this.CurrentInstance.GetType().FullName + "'.");

            // change the destination of the listing:
            if (!string.IsNullOrEmpty(destination))
                SetOutputChannel(destination);

            if (_interval_for_collection.HasValue)
            {
                _start_marker = DateTime.Now;
                _end_marker = _start_marker.AddDays(_interval_for_collection.Value.Days);
                _end_marker = _start_marker.AddHours(_interval_for_collection.Value.Hours);
                _end_marker = _start_marker.AddMinutes(_interval_for_collection.Value.Minutes);
                _end_marker = _start_marker.AddSeconds(_interval_for_collection.Value.Seconds);
                _end_marker = _start_marker.AddMilliseconds(_interval_for_collection.Value.Milliseconds);
            }
        }

        private void SendToLocalStorage(IEnvelope message)
        {
            var payload = message.Body.GetPayload<T>();

            if (this.EnsureUniqueItems || _ensure_unique_items)
            {
                if (!this.AccumulatedItemsStorage.Contains(payload))
                    this.AccumulatedItemsStorage.Add(payload);
            }
            else
            {
                AccumulatedItemsStorage.Add(payload);
            }
        }

        private void DispatchMessagesToChannel()
        {

            if (OutputChannel is NullChannel)
                throw new Exception("No output channel was set to deliver the accumulated messages to.");

            var items = new List<T>(this.AccumulatedItemsStorage);

            if (_send_as_batch)
                OutputChannel.Send(new Envelope(items.ToArray()));
            else
            {
                // parallels here?
                foreach (var item in items)
                {
                    try
                    {
                        OutputChannel.Send(new Envelope(item));
                    }
                    catch (Exception exception)
                    {
                        continue;
                    }
                }
            }
        }

        private void OnAccumulatorStrategyCompleted(string channel, IEnvelope message)
        {

            try
            {
                _current_batch_size = 0;
                if (this.CleanUpAction != null)
                    this.CleanUpAction.Invoke();
            }
            catch
            {

            }

            // re-direct the message:
            if (!(message is NullEnvelope))
            {
                EventHandler<MessageHandlingStrategyCompleteEventArgs> evt = this.ChannelStrategyCompleted;

                if (evt != null)
                    evt(this, new MessageHandlingStrategyCompleteEventArgs(channel, message));
            }
        }

        private TimeSpan? CreateTimespanFromInterval(string interval)
        {
            var timeParts = interval.Split(new char[] {':'});
            if(timeParts.Length != 4) return null;
            
            var days = Convert.ToInt32(timeParts[0]);
            var hours = Convert.ToInt32(timeParts[1]);
            var minutes = Convert.ToInt32(timeParts[2]);
            var seconds = Convert.ToInt32(timeParts[3]);

            var timespan = new TimeSpan(days, hours, minutes, seconds);

            return timespan;
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