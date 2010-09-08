using System;
using System.Collections.Generic;
using System.Text;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Core.Internals.Reflection;
using Carbon.Integration.Stereotypes.Accumulator.Impl;

namespace Carbon.Integration.Stereotypes.Accumulator
{
    /// <summary>
    /// The accumulator attribute is used for a method that will take a series of 
    /// input messages and will be triggered to send the complete listing to an 
    /// channel when the completion criteria is met.
    /// </summary>
    /// <example>
    /// 
    /// [MessageEndpoint("orders")]
    /// public class NewOrderAccumulator 
    /// {
    ///     [Accumulator(typeof(NewOrderAccumulatorStrategy), "billing"]
    ///     public void AcceptOrder(CreateOrderMessage message)
    ///     {
    ///         // let the accumulator keep track of the messages
    ///         // until the completion criteria is satisfied:
    ///     }
    /// }
    /// 
    /// // here is the class that implements the strategy 
    /// // for the accumulator:
    /// public class NewOrderAccumulatorStrategy : AbstractAccumulatorMessageHandlingStrategy{CreateOrderMessage}
    /// {
    ///     private static IList{CreateOrderMessage} _items;
    /// 
    ///     public NewOrderAccumulatorStratey()
    ///     {
    ///           if(_items == null)
    ///               _items = new List{CreateOrderMessage}();
    ///           
    ///           // pass down the local persistant storage to the strategy:        
    ///           SetStorage(_items);
    /// 
    ///           MessagesInBatch = 5; // number of messages to receive from source (part of completion criteria)
    ///     }
    /// }
    ///
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AccumulatorAttribute : Attribute, IMessageHandlingStrategyAttribute
    {
        public Type Strategy { get; set; }


        public string ChannelToSendListing { get; set; }

        /// <summary>
        /// Number of messages to accumulate in one instance (max limit is 256 messages per accumulation).
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Period of time to conduct the accumulation. All accumulated messages will be sent when interval has passed.
        /// </summary>
        public string IntervalForCollection { get; set; }

        /// <summary>
        /// Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)
        /// </summary>
        public bool SendResultAsBatch { get; set; }

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="typeForAccumulation"></param>
        /// <param name="sendResultAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="batchSize">Number of messages to accumulate in one instance (max limit is 256 messages per accumulation).</param>
        /// <param name="interval">Period of time to conduct the accumulation. All accumulated messages will be sent when interval has passed (format: dd:hh:mm:ss) .</param>
        public AccumulatorAttribute(Type typeForAccumulation, bool sendResultAsBatch, int batchSize, string interval)
        {
            var reflection = new DefaultReflection();
            var instanceType = reflection.GetGenericVersionOf(typeof(DefaultAccumulatorMessageHandlingStrategy<>),
                                                              typeForAccumulation);

            Strategy = reflection.BuildInstance(instanceType).GetType();
            SendResultAsBatch = sendResultAsBatch;

            BatchSize = batchSize;
            IntervalForCollection = interval;
        }

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="accumulatorStrategy">Strategy to handle the message accumulation</param>
        /// <param name="sendAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        public AccumulatorAttribute(Type accumulatorStrategy, bool sendAsBatch)
        {
            Strategy = accumulatorStrategy;
            SendResultAsBatch = sendAsBatch;
        }

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="accumulatorStrategy">Strategy to handle the message accumulation</param>
        /// <param name="sendAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="channelToSendListing">Channel name to send the compiled listing to for processing.</param>
        //public AccumulatorAttribute(Type accumulatorStrategy, bool sendAsBatch, string channelToSendListing)
        //{
        //    Strategy = accumulatorStrategy;
        //    ChannelToSendListing = channelToSendListing;
        //    IntervalForCollection = null;
        //    SendResultAsBatch = sendAsBatch;
        //}

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="accumulatorStrategy">Strategy to handle the message accumulation</param>
        /// <param name="sendAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="batchSize">Number of messages to accumulate in one instance.</param>
        public AccumulatorAttribute(Type accumulatorStrategy, bool sendAsBatch, int batchSize)
        {
            Strategy = accumulatorStrategy;
            BatchSize = batchSize;
            SendResultAsBatch = sendAsBatch;
        }

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="accumulatorStrategy">Strategy to handle the message accumulation</param>
        /// <param name="sendAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="channelToSendListing">Channel name to send the compiled listing to for processing.</param>
        /// <param name="batchSize">Number of messages to accumulate in one instance.</param>
        public AccumulatorAttribute(Type accumulatorStrategy, bool sendAsBatch, string channelToSendListing, int batchSize)
        {
            Strategy = accumulatorStrategy;
            ChannelToSendListing = channelToSendListing;
            BatchSize = batchSize;
            SendResultAsBatch = sendAsBatch;
        }

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="accumulatorStrategy">Strategy to handle the message accumulation</param>
        /// <param name="sendAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="interval">Period of time to conduct the accumulation. All accumulated messages will be sent when interval has passed (format: dd:hh:mm:ss) .</param>
        public AccumulatorAttribute(Type accumulatorStrategy, bool sendAsBatch, string interval)
        {
            Strategy = accumulatorStrategy;
            IntervalForCollection = interval;
            SendResultAsBatch = sendAsBatch;
        }

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="accumulatorStrategy">Strategy to handle the message accumulation</param>
        /// <param name="sendAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="channelToSendListing">Channel name to send the compiled listing to for processing.</param>
        /// <param name="interval">Period of time to conduct the accumulation. All accumulated messages will be sent when interval has passed (format: dd:hh:mm:ss) .</param>
        public AccumulatorAttribute(Type accumulatorStrategy, bool sendAsBatch, string channelToSendListing, string interval)
        {
            Strategy = accumulatorStrategy;
            ChannelToSendListing = channelToSendListing;
            IntervalForCollection = interval;
            SendResultAsBatch = sendAsBatch;
        }
    }
}