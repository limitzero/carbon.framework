using System;
using System.Collections.Generic;
using System.Text;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Gateway
{
    /// <summary>
    /// Attribute for indicating that this will act as a application message gateway.
    /// </summary>
    /// <example>
    /// (1). Simple one-way gateway to send a message to a channel
    /// 
    /// public interface IInvoiceProcessor
    /// {
    ///     [Gateway("newInvoice")]
    ///     void ProcessInvoice(Invoice invoice);
    /// }
    /// 
    /// (2). Request-Response gateway to send a message to a channel and get the result
    /// on a differing channel (in this case it will poll indefinitely for the response)
    /// 
    /// public interface IInvoiceProcessor
    /// {
    ///     [Gateway("newInvoice","invoiceConfirmation")]
    ///     InvoiceConfirmation ProcessInvoice(Invoice invoice);
    /// }
    /// 
    /// (3). Request-Response gateway to send a message to a channel and get the result
    /// on a differing channel and expect the response within a certain timeframe
    /// 
    /// public interface IInvoiceProcessor
    /// {
    ///     [Gateway("newInvoice","invoiceConfirmation", 10)]
    ///     InvoiceConfirmation ProcessInvoice(Invoice invoice);
    /// }
    /// 
    /// </example>
    public class GatewayAttribute : Attribute
    {
        /// <summary>
        /// (Read-Only). Name of the channel to send the request to for processing.
        /// </summary>
        public string RequestChannel { get; private set; }

        /// <summary>
        /// (Read-Only). Name of the channel to poll for a reply for the request.
        /// </summary>
        public string ReplyChannel { get; private set; }

        /// <summary>
        /// (Read-Write). Time to wait, in seconds, for the reply
        /// </summary>
        public int ReplyTimeout { get; private set; }

        /// <summary>
        /// This will act as an application message gateway with a channel to send 
        /// the request.
        /// </summary>
        /// <param name="requestChannel">Name of the channel to send request.</param>
        public GatewayAttribute(string requestChannel)
            : this(requestChannel, string.Empty, 0)
        {
        }

        /// <summary>
        /// This will act as an application message gateway with a channel to send 
        /// the request and a channel to receive the reply.
        /// </summary>
        /// <param name="requestChannel">Name of the channel to send request.</param>
        /// <param name="replyChannel">Name of the channel to poll for a reply</param>
        public GatewayAttribute(string requestChannel, string replyChannel)
            :this(requestChannel, replyChannel, Int32.MaxValue - 1)
        {
        }

        /// <summary>
        /// This will act as an application message gateway with a channel to send 
        /// the request and a channel to receive the reply along with a time in seconds
        /// to wait for the reply.
        /// </summary>
        /// <param name="requestChannel">Name of the channel to send request.</param>
        /// <param name="replyChannel">Name of the channel to poll for a reply</param>
        /// <param name="replyTimeout">Number of seconds to wait for the reply.</param>
        public GatewayAttribute(string requestChannel, string replyChannel, int replyTimeout)
        {
            RequestChannel = requestChannel;
            ReplyChannel = replyChannel;
            ReplyTimeout = replyTimeout;
        }
    }
}