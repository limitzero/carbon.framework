using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.Core;
using Carbon.Integration.Configuration;
using Carbon.Integration.Dsl.Surface.Registry;
using Carbon.Test.Domain.Gateways;
using Carbon.Test.Domain.Messages;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Xunit;
using Carbon.Test.Domain.Surfaces;

namespace Carbon.Integration.Tests
{
    public class IntegrationContextTests
    {
        readonly IWindsorContainer container;

        public IntegrationContextTests()
        {
            container = new WindsorContainer(new XmlInterpreter());
            container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());
        }

        [Fact]
        public void Can_load_integration_context_from_facility()
        {
            var context = container.Resolve<IIntegrationContext>();
            Assert.NotNull(context);
        }

        [Fact]
        public void Can_get_gateway_implementation_from_configuration()
        {
            var context = container.Resolve<IIntegrationContext>();
            context.LoadAllSurfaces();
            context.Start();

            var gateway = context.GetComponent<IInvoiceProcessorGateway>();
            Assert.NotNull(gateway);

            gateway.Process(new Invoice());

            context.Stop();
        }

        [Fact]
        public void Can_get_gateway_implementation_from_configuration_for_request_response()
        {
            using (var context = container.Resolve<IIntegrationContext>())
            {
                context.LoadSurface(typeof(ProcessTradesSurface));
                context.Start();

                var surface = context.GetComponent<ProcessTradesSurface>();
                Console.WriteLine(surface.Verbalize());

                var gateway = context.GetComponent<ITradesGateway>();
                Assert.NotNull(gateway);

                var trade = gateway.ProcessTrade(new TradeRequest());
                Assert.NotNull(trade);
            }
        }

        [Fact]
        public void Can_get_integration_context_from_facility_and_load_surface_for_testing()
        {
            var context = container.Resolve<IIntegrationContext>();
            context.LoadSurface(typeof (TestIntegrationSurface));

            var registry = container.Resolve<ISurfaceRegistry>();

            Assert.Equal(1, registry.Surfaces.Count());
            Assert.Equal(typeof(TestIntegrationSurface), registry.Surfaces[0].GetType());
        }

        [Fact]
        public void Can_start_integration_context_and_scheduler_deliver_messages_to_endpoints()
        {
            using(var context = container.Resolve<IIntegrationContext>())
            {
                context.LoadSurface<TestIntegrationSurface>();
                context.Start();
                
                // scheduler should pick up components for sending messages
                // and generate new messages to be passed to the 'TestComponent'
                // class instance:
                var surface = context.GetComponent<TestIntegrationSurface>();
                Console.WriteLine(surface.Verbalize());

                var wait = new ManualResetEvent(false);
                wait.WaitOne(TimeSpan.FromSeconds(10));
                wait.Set();
            }
        }

        [Fact]
        public void Can_get_integration_context_from_facility_and_start_session_for_sending_and_receiving_messages()
        {
            using(var context = container.Resolve<IIntegrationContext>())
            {
                context.LoadAllSurfaces();
                context.Start();
                
                //context.Send(new Uri(@"file://c:\trash\incoming"), new Envelope("This is a test message"));
                context.Send(new Uri(@"file://c:\trash\mainframe\inbound"), new Envelope("This is a test message"));

                var wait = new ManualResetEvent(false);
                wait.WaitOne(TimeSpan.FromSeconds(10));
                wait.Set();
            }
        }
    }
}
