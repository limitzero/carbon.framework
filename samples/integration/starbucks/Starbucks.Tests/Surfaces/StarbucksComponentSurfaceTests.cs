using System;
using System.Threading;
using Carbon.Integration.Testing;
using Starbucks.Messages;
using Starbucks.Surfaces;
using Xunit;
using Carbon.Integration.Dsl.Surface.Registry;
using Starbucks.Surfaces.Customer.Components;

namespace Starbucks.Tests.Surfaces
{
    public class StarbucksComponentSurfaceTests : BaseMessageConsumerTestFixture
    {
        public StarbucksComponentSurfaceTests()
            : base(@"empty.config.xml")
        {
            Context.LoadSurface<StarbucksComponentSurface>();
            Context.Start();
        }

        [Fact]
        public void can_register_surface_and_create_verbalization_of_configuration()
        {
            var surface = Context.GetComponent<ISurfaceRegistry>().Find<StarbucksComponentSurface>();
            Assert.NotNull(surface);

            surface.Configure();
            Console.WriteLine(surface.Verbalize());
        }

        [Fact]
        public void can_place_an_order_and_receive_the_drinks_from_the_barrista_when_the_order_is_completed()
        {
            var gateway = Context.GetComponent<ICustomerGateway>();
            gateway.PlaceOrder(new CreateDrinkOrderMessage());

            var wait = new ManualResetEvent(false);
            wait.WaitOne(TimeSpan.FromSeconds(2));
            wait.Set();
        }
    }
}