using Carbon.Integration.Testing;
using Xunit;

namespace Starbucks.Tests.Surfaces.Customer
{
    public class when_an_order_is_placed_for_coffee : BaseMessageConsumerTestFixture
    {
        public when_an_order_is_placed_for_coffee()
        {
            
        }

        [Fact]
        public void then_the_gateway_will_be_created_to_send_a_message_the_the_new_order_channel()
        {

        }
    }
}