using System;
using Carbon.Core;
using Carbon.Integration.Stereotypes.Consumes;
using Starbucks.Messages;

namespace Starbucks.Surfaces.Cashier.Components
{
    /// <summary>
    /// This is the message consumer that will fulfill the role of the "Cashier".
    /// </summary>
    public class CashierMessageConsumer
        : ICanConsumeAndReturn<CreateDrinkOrderMessage, PaymentNeededForOrderMessage>, 
          ICanConsumeAndReturn<SubmitPaymentMessage, PrepareOrderMessage>
    {
        /// <summary>
        /// This will take the new coffee order from the customer and forward the payment
        /// message back to the customer before the barrista will make the items on the order.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Consumes("customer")]
        public PaymentNeededForOrderMessage Consume(CreateDrinkOrderMessage message)
        {
            var pn = new PaymentNeededForOrderMessage();
            return pn;
        }

        /// <summary>
        /// This is the component that will receive the payment from the customer for the given order
        /// and signal the creation of the drinks.
        /// </summary>
        /// <param name="message"></param>
        [Consumes("barrista")]
        public PrepareOrderMessage Consume(SubmitPaymentMessage message)
        {
            var msg = new PrepareOrderMessage();
            return msg;
        }


    }
}