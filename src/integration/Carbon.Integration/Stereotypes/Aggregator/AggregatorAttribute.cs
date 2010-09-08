using System;
using System.Collections.Generic;
using System.Text;
using Carbon.Core.Stereotypes.For.MessageHandling;

namespace Carbon.Integration.Stereotypes.Aggregator
{
    /// <summary>
    /// Attribute for a indicating that a series of messages will be distilled into one message.
    /// </summary>
    /// <example>
    /// (1). Aggregating an enumerable collection of orders to find the ones that are "completed":
    /// 
    /// [MessagingEndpoint("billing", "shipping")]
    /// public class OrderToInvoiceAggregator
    /// {
    ///   
    ///     [Aggregator(typeof(OrderToInvoiceAggregatorStrategy)]    
    ///     public void ProcessCompletedOrdersForInvoice(IList{Order} orders)
    ///     {}
    /// }
    /// 
    ///  public class OrderToInvoiceAggregatorStrategy : AbstractAggregatorMessageHandlingStrategy{Order}
    ///  {
    ///      // only select the completed orders:
    ///      public override Order DoAggregatorStrategy(IMessage message)
    ///      {
    ///          Order orderToReturn = null;
    ///          
    ///          var order = message.Body.GetPayload{Order}();
    ///          if(order.Status == OrderStatus.Completed)
    ///              orderToReturn = order;
    ///              
    ///          return orderToReturn;
    ///      }
    ///      
    ///      // send the invoice out to the channel for processing:
    ///      public IMessage DoAggregationStrategy(IMessage message)
    ///      {
    ///          var invoice = new Invoice();
    ///          
    ///          foreach(var order in base.GetAccumulatedOutput())
    ///              invoice.AddOrder(order);
    ///          
    ///          message.Body.SetPayload(invoice); 
    ///          
    ///          return message;
    ///      }
    ///  }
    /// 
    /// (2). Aggregating a message that contains an enumberable property for the orders
    /// to be inspected for "completed" status (if this case you could deliver invoices as the 
    ///  orders are completed..both 1 & 2 are the same in effect):
    /// 
    /// [MessagingEndpoint("billing", "shipping")]
    /// public class OrderToInvoiceAggregator
    /// {
    ///     // the invoice will have a property called "Orders" that is 
    ///     // enumerable and will be inspected individually for "completed"
    ///     // status:
    ///     [Aggregator(typeof(OrderToInvoiceAggregatorStrategy,"Orders")]    
    ///     public void ProcessCompletedOrdersForInvoice(Invoice message)
    ///     {}
    /// }
    /// 
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AggregatorAttribute : Attribute, IMessageHandlingStrategyAttribute
    {
        /// <summary>
        /// (Read-Only). The type of the component used for custom message aggregation.
        /// </summary>
        public Type Strategy { get; set; }

        /// <summary>
        /// (Read-Only). The name of the property containing enumerated values on the message 
        /// being passed to the method used for aggregation.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// This will conduct the message aggregation strategy for the enumerable message.
        /// </summary>
        /// <param name="strategy">Type for the component that will handle the custom aggregation strategy.</param>
        public AggregatorAttribute(Type strategy)
        {
            //if (!typeof(BaseAggregatorStrategy<>).IsAssignableFrom(strategy))
            //    throw new Exception("The strategy used must derive from " + typeof(BaseAggregatorStrategy<>).Name);

            Strategy = strategy;
        }

        /// <summary>
        /// This will conduct the message aggregation strategy for the enumerable message.
        /// </summary>
        /// <param name="strategy">Type for the component that will handle the custom aggregation strategy.</param>
        /// <param name="propertyContainingEnumeratedValues">Property name on the message that will contain the enumrated values if the message itself is not assignable from IEnumerable.</param>
        public AggregatorAttribute(Type strategy, string propertyContainingEnumeratedValues)
        {
            //if (!typeof(BaseAggregatorStrategy<>).IsAssignableFrom(strategy))
            //    throw new Exception("The strategy used must derive from " + typeof(BaseAggregatorStrategy<>).Name);

            Strategy = strategy;
            PropertyName = propertyContainingEnumeratedValues;
        }
    }
}