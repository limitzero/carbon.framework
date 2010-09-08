using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Integration.Stereotypes.Splitter.Impl;

namespace Carbon.Integration.Stereotypes.Splitter
{
    /// <summary>
    /// Attribute for a indicating that a composed message will be split into a smaller parts and sent one by one to a channel for processing.
    /// </summary>
    /// <example>
    /// [MessagingEndpoint("billing", "shipping")]
    /// public class BilledOrderSplitter
    /// {
    ///   
    ///     [Splitter(typeof(OrderSplitterStrategy)]    
    ///     public IList{Orders} SplitOrder(IList{Order} orders)
    ///     {
    ///           return orders;
    ///     }
    /// }
    /// 
    /// public class OrderSplitterStrategy : SplitterMessageHandlingStrategy
    /// {
    ///     public override DoSplitterStrategy(IEnvelope message)
    ///     {
    ///        // split the order according to rules (one at  time):
    ///        var order = message.Body.GetPayload{Order}();
    /// 
    ///        if(order.IsComplete)
    ///       {
    ///            // call the base component to let it know that 
    ///           //  that the de-composed message is ready to send:
    ///           // (the message will be sent out on the "shipping" 
    ///           // channel)
    ///
    ///           base.OnMessageSplit(new Envelope(order));
    ///        }
    ///     }
    /// }
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SplitterAttribute : Attribute, IMessageHandlingStrategyAttribute
    {
        public Type Strategy { get; set; }
        public string OutputChannelName { get; set; }

        public SplitterAttribute()
            : this(new DefaultSplitterMessageHandlingStrategy().GetType())
        {
        }

        public SplitterAttribute(string outputChannelName)
            : this(new DefaultSplitterMessageHandlingStrategy().GetType(), outputChannelName)
        {
           
        }

        public SplitterAttribute(Type strategy)
            : this(strategy, string.Empty)
        {
        }

        public SplitterAttribute(Type strategy, string outputChannelName)
        {
            if (!typeof(AbstractSplitterMessageHandlingStrategy).IsAssignableFrom(strategy))
                throw new Exception("The strategy used for message splitting must derive from " + typeof(AbstractSplitterMessageHandlingStrategy).Name);

            Strategy = strategy;
            OutputChannelName = outputChannelName;
        }
    }
}
