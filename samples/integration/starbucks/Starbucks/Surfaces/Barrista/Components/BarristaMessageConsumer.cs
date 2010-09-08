using System;
using Carbon.Core;
using Starbucks.Messages;
using Carbon.Integration.Stereotypes.Consumes;
using Carbon.Core.Adapter.Template;

namespace Starbucks.Surfaces.Barrista.Components
{
    /// <summary>
    /// This is the message consumer that will fulfill the role of the "Barrista".
    /// </summary>
    public class BarristaMessageConsumer
        : ICanConsumeAndReturn<PrepareOrderMessage, OrderPreparedMessage>
    {
        private readonly IAdapterMessagingTemplate _template;

        public BarristaMessageConsumer(IAdapterMessagingTemplate template)
        {
            _template = template;
        }

        /// <summary>
        /// This will signal to the customer that the order has been completed.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Consumes("customer")]
        public OrderPreparedMessage Consume(PrepareOrderMessage message)
        {
            var logUri = new Uri(Constants.LogUris.DEBUG_LOG_URI);

            _template.DoSend(logUri, new Envelope("Preparing drinks...."));

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));

            _template.DoSend(logUri, new Envelope("Drinks prepared."));

            var msg = new OrderPreparedMessage();
            return msg;
        }
    }
}