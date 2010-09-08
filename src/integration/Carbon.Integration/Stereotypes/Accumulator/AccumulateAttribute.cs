using System;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Integration.Stereotypes.Accumulator.Impl;

namespace Carbon.Integration.Stereotypes.Accumulator
{
    /// <summary>
    /// The accumulate attribute is used for a method that will take a series of 
    /// input messages and will be triggered to send the complete listing to an 
    /// channel when the completion criteria is met. It uses a default implementation 
    /// for accumulating messages of your choice. In contrast, the <see cref="AccumulatorAttribute">
    /// aacumulator</see> must be supplied with a user-defined strategy for accumulation and 
    /// can contain business logic for accumulation.
    /// </summary>
    /// <example>
    /// 
    /// [MessageEndpoint("orders","shipping")]
    /// public class NewOrderAccumulator 
    /// {
    ///     // here we will use the underlying default implementation of 
    ///     // accumulating messages by just specifying the type to accumulate
    ///    //  and the number of messages expected and whether or not to send 
    ///    // the resultant collection as a whole (true) or send each individual message (false) 
    ///    // to the output channel
    ///     [Accumulate(typeof(CreateOrderMessage), true, 10]
    ///     public void AcceptOrder(CreateOrderMessage message)
    ///     {
    ///         // let the accumulator keep track of the messages
    ///         // until the completion criteria is satisfied
    ///         // namely 10 messages are returned
    ///     }
    /// 
    ///     // here we will use the underlying default implementation of 
    ///     // accumulating messages by just specifying the type to accumulate
    ///    //  and the number of messages expected and whether or not to send 
    ///    // the resultant collection as a whole (true) or send each individual message (false) 
    ///    // to the output channel
    ///     [Accumulate(typeof(CreateOrderMessage), true, "00:00:00:10"]
    ///     public void AcceptOrder(CreateOrderMessage message)
    ///     {
    ///         // let the accumulator keep track of the messages
    ///         // until the completion criteria is satisfied
    ///         // namely waiting ten seconds for all messages to be
    ///        //  sent to the accumulator.
    ///     }
    /// }
    ///</example>
    [System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AccumulateAttribute : Attribute, IMessageHandlingStrategyAttribute
    {
        public Type Strategy { get; set;}

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
        /// Flag to indicate that unique items should be stored.
        /// </summary>
        public bool EnsureUniqueItems { get; set; }
       
        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="typeForAccumulation"></param>
        /// <param name="sendResultAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="batchSize">Number of messages to accumulate in one instance (max limit is 256 messages per accumulation).</param>
        public AccumulateAttribute(Type typeForAccumulation, bool sendResultAsBatch, int batchSize)
            :this(typeForAccumulation, sendResultAsBatch, batchSize, false, null)
        {

        }

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="typeForAccumulation"></param>
        /// <param name="sendResultAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="batchSize">Number of messages to accumulate in one instance (max limit is 256 messages per accumulation).</param>
        /// <param name="ensureUniqueItems">Flag to indicate that unique messages should be stored.</param>
        public AccumulateAttribute(Type typeForAccumulation, bool sendResultAsBatch, int batchSize, bool ensureUniqueItems)
            : this(typeForAccumulation, sendResultAsBatch, batchSize, ensureUniqueItems, null)
        {

        }

        /// <summary>
        /// This will indicate the component to handle the message accumulation and forward 
        /// the results to a channel upon the logic for message completion.
        /// </summary>
        /// <param name="typeForAccumulation"></param>
        /// <param name="sendResultAsBatch">Flag to indicate whether to send all messages out on the channel one-by-one(false) or send the complete listing as an enumerable object array {i.e. object[]}(true)</param>
        /// <param name="batchSize">Number of messages to accumulate in one instance (max limit is 256 messages per accumulation).</param>
        /// <param name="ensureUniqueItems">Flag to indicate that unique messages should be stored.</param>
        /// <param name="interval">Period of time to conduct the accumulation. All accumulated messages will be sent when interval has passed (format: dd:hh:mm:ss) .</param>
        public AccumulateAttribute(Type typeForAccumulation, bool sendResultAsBatch, int batchSize, bool ensureUniqueItems, string interval)
        {
            var reflection = new DefaultReflection();
            var instanceType = reflection.GetGenericVersionOf(typeof(DefaultAccumulatorMessageHandlingStrategy<>),
                                                              typeForAccumulation);

            Strategy = reflection.BuildInstance(instanceType).GetType();
            SendResultAsBatch = sendResultAsBatch;
            EnsureUniqueItems = ensureUniqueItems;
            BatchSize = batchSize;
            IntervalForCollection = interval;
        }


    }
}