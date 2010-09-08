using Carbon.Integration.Testing;
using Starbucks.Messages;
using Starbucks.Surfaces.Customer.Components;
using Xunit;

namespace Starbucks.Tests.Surfaces.Customer
{
    public class when_an_order_is_placed_for_coffee : BaseMessageConsumerTestFixture
    {
        private string _cashier_channel = "cashier";

        public when_an_order_is_placed_for_coffee()
            :base(@"empty.config.xml")
        {
            CreateChannels(_cashier_channel);
            RegisterGateway<ICustomerGateway>(_cashier_channel, "PlaceOrder");
        }

        [Fact]
        public void then_the_gateway_will_be_created_to_send_the_new_order_message_to_the_cashier()
        {
            // assert that the gateway can be created:
            var gateway = Context.GetComponent<ICustomerGateway>();
            Assert.NotNull(gateway);

            // place the order...
            gateway.PlaceOrder(new CreateDrinkOrderMessage());

            // assert that the message was placed on the channel for the cashier to process:
            var message = ReceiveMessageFromChannel<CreateDrinkOrderMessage>(_cashier_channel, null);
            Assert.Equal(typeof(CreateDrinkOrderMessage), message.GetType());
        }
    }


}