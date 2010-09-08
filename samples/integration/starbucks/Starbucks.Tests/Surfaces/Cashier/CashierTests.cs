using System.Threading;
using Carbon.Core;
using Carbon.Integration.Testing;
using Starbucks.Surfaces.Cashier.Components;
using Xunit;
using Starbucks.Messages;
using System;

namespace Starbucks.Tests.Surfaces.Cashier
{
    public class when_the_new_order_message_is_received_from_the_customer : BaseMessageConsumerTestFixture
    {
        private ManualResetEvent _wait = null;
        private string _cashier_channel = "cashier";
        private string _customer_channel = "customer";
        private string _barrista_channel = "barrista";

        public when_the_new_order_message_is_received_from_the_customer()
            : base(@"empty.config.xml")
        {
            CreateChannels(_cashier_channel, _customer_channel, _barrista_channel);
            RegisterComponent<CashierMessageConsumer>(_cashier_channel);

            // send the message to the consumer over the channel:
            _wait = new ManualResetEvent(false);

            var message = new Envelope(new CreateDrinkOrderMessage());
            Context.Send(_cashier_channel, message);

            _wait.WaitOne(TimeSpan.FromSeconds(2));
            _wait.Set();
        }

        [Fact]
        public void it_will_take_the_order_from_the_customer_and_send_a_payment_notice_message_back_to_the_customer()
        {
            // make sure the consumer's current input message is the one that we want to process:
            Assert.Equal(typeof(CreateDrinkOrderMessage), MessageConsumerInputMessage.GetType());

            // make sure that the message indicating payment is sent to the customer:
            var paymentNeededMessage = ReceiveMessageFromChannel<PaymentNeededForOrderMessage>(_customer_channel, null);
            Assert.Equal(typeof(PaymentNeededForOrderMessage), paymentNeededMessage.GetType());
        }

    }
    
    public class when_the_message_for_order_payment_is_received_from_the_customer : BaseMessageConsumerTestFixture
    {
        private ManualResetEvent _wait = null;
        private string _cashier_channel = "cashier";
        private string _customer_channel = "customer";
        private string _barrista_channel = "barrista";

        public when_the_message_for_order_payment_is_received_from_the_customer()
            : base(@"empty.config.xml")
        {
            CreateChannels(_cashier_channel, _customer_channel, _barrista_channel);
            RegisterComponent<CashierMessageConsumer>(_cashier_channel);

            // send the message to the consumer over the channel:
            _wait = new ManualResetEvent(false);

            var message = new Envelope(new SubmitPaymentMessage());
            Context.Send(_cashier_channel, message);

            _wait.WaitOne(TimeSpan.FromSeconds(2));
            _wait.Set();
        }

        [Fact]
        public void it_will_take_the_payment_submitted_message_and_forward_the_order_to_the_barrista_for_processing()
        {
            // make sure the consumer's current input message is the one that we want to process:
            Assert.Equal(typeof(SubmitPaymentMessage), MessageConsumerInputMessage.GetType());

            // make sure that the barrista is notified that the order is to be created (after payment of course):
            var prepareOrderMessage = ReceiveMessageFromChannel<PrepareOrderMessage>(_barrista_channel, null);
            Assert.Equal(typeof(PrepareOrderMessage), prepareOrderMessage.GetType());
        }

    }



}