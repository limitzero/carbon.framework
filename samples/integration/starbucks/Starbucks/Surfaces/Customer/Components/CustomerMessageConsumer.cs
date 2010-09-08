using System;
using Carbon.Core;
using Starbucks.Messages;
using Carbon.Integration.Stereotypes.Consumes;
using Carbon.Core.Adapter.Template;

namespace Starbucks.Surfaces.Customer.Components
{
    /// <summary>
    /// This is the message consumer that will fulfill the role of the "Customer".
    /// </summary>
    public class CustomerMessageConsumer
        : ICanConsumeAndReturn<PaymentNeededForOrderMessage, SubmitPaymentMessage>, 
         ICanConsume<OrderPreparedMessage>
    {
        private readonly IAdapterMessagingTemplate _template;

        public CustomerMessageConsumer(IAdapterMessagingTemplate template)
        {
            _template = template;
        }

        [Consumes("cashier")]
        public SubmitPaymentMessage Consume(PaymentNeededForOrderMessage message)
        {
            // create the message that the customer will aggree to pay for the order:
            var msg = new SubmitPaymentMessage();
            return msg;
        }

        public void Consume(OrderPreparedMessage message)
        {
            // do nothing here but enjoy the coffee!!!
            var logUri = new Uri(Constants.LogUris.DEBUG_LOG_URI);
            _template.DoSend(logUri, new Envelope("Enjoying my order...."));
        }
    }
}