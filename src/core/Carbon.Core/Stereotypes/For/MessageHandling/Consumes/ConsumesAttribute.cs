using System;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Stereotypes.For.MessageHandling.Consumes.Impl;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Consumes
{
    /// <summary>
    /// Attribute for general message consumers/handlers where the response 
    /// can be re-directed to another output channel that is different 
    /// from the one specified on the <seealso cref="MessageEndpointAttribute">message endpoint</seealso>
    /// annotation.
    /// </summary>
    /// <code>
    /// [MessageEndpoint("orders","billing")]
    /// public class OrderProcessor
    /// {
    ///     [Consumes("deliveries")]
    ///     public Invoice ProcessOrder(Order order)
    ///     {
    ///         // the invoice will be sent to the "deliveries" channel
    ///         // instead of teh "billing" channel:
    ///         return new Invoice();
    ///     }
    /// }
    /// </code>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ConsumesAttribute : Attribute, IMessageHandlingStrategyAttribute
    {
        ///<summary>
        /// The name of the channel to route the results of the method to.
        ///</summary>
        public string OutputChannel { get; set; }

        /// <summary>
        /// (Read-Write). The type corresponding to the component that will implement the strategy 
        /// for handling the message that is different from the message endpoint component.
        /// </summary>
        public Type Strategy { get; set; }

        public ConsumesAttribute()
            :this(string.Empty)
        {
        }

        ///<summary>
        /// This will indicate that the message passed to the method can be processed
        /// and the return value is routed to a different channel than the one specified
        /// by the <seealso cref="MessageEndpointAttribute">message end point</seealso> value.
        ///</summary>
        ///<param name="outputChannel">Name of the channel to re-direct the message to.</param>
        public ConsumesAttribute(string outputChannel)
        {
            OutputChannel = outputChannel;
            Strategy = typeof (ConsumesMessageHandlingStrategy);
        }
 
    }
}