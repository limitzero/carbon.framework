using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Configuration;
using Carbon.Integration.Stereotypes.Aggregator;
using Carbon.Integration.Stereotypes.Aggregator.Impl;
using Castle.Windsor;
using Xunit;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace Carbon.Integration.Tests.Stereotypes.Aggregator
{
    public class AggregatorTests
    {
        private IWindsorContainer _container = null;
        private IIntegrationContext _context = null;

        public AggregatorTests()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            _container = new WindsorContainer(@"empty.config.xml");
            _container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            _context = _container.Resolve<IIntegrationContext>();

            // add the channels to the infrastructure:
            _context.GetComponent<IChannelRegistry>().RegisterChannel("orders");
            _context.GetComponent<IChannelRegistry>().RegisterChannel("completed_orders");

            // register the component and create the message endpoint for it:
            _context.GetComponent<IObjectBuilder>().Register(typeof(CompletedOrdersAggregator).Name, typeof(CompletedOrdersAggregator));
            _context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint("orders", string.Empty, string.Empty, new CompletedOrdersAggregator());
        }

        [Fact]
        public void can_aggregate_a_series_of_messages_into_one_composite_message_and_forward_to_appropriate_channel()
        {
            // send the message over the channel to the endpoint via the message endpoint activator:
            var orders = new List<Order>();
            for (var i = 1; i <= 10; i++)
                orders.Add(new Order() { Id = i, IsCompleted = i % 2 == 0 }); // 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 => 2, 4, 6, 8, 10

            _context.GetComponent<IChannelRegistry>().FindChannel("orders").Send(new Envelope(orders));

            // wait for the message to appear on the channel:
            var wait = new ManualResetEvent(true);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            var completedOrdersChannel = _context.GetComponent<IChannelRegistry>().FindChannel("completed_orders");
            Assert.NotEqual(typeof(NullChannel), completedOrdersChannel.GetType());

            var shippingInvoice = completedOrdersChannel.Receive().Body.GetPayload<ShippingInvoice>();
            Assert.Equal(5, shippingInvoice.Orders.Count);
        }

        [Message]
        public class ShippingInvoice
        {
            private List<Order> _orders = new List<Order>();

            public ReadOnlyCollection<Order> Orders
            {
                get;
                private set;
            }

            public void AddOrder(Order order)
            {
                _orders.Add(order);
                this.Orders = _orders.AsReadOnly();
            }
        }

        [Message]
        public class Order
        {
            public int Id { get; set; }
            public bool IsCompleted { get; set; }
        }

        [MessageEndpoint("orders", "completed_orders")]
        public class CompletedOrdersAggregator
        {
            [Aggregator(typeof(CompletedOrdersAggregatorStrategy))]
            public void AggregateOrders(List<Order> orders)
            {
                // the list of orders will be passed in and 
                // picked up by the aggregation routine
            }
        }

        public class CompletedOrdersAggregatorStrategy : 
            AbstractAggregatorMessageHandlingStrategy<Order>
        {
            /// <summary>
            /// This is the point in which each individual message
            /// will be checked to see if it can be stored for 
            /// creating the single message for subsequent processing.
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            public override Order DoAggregatorStrategy(IEnvelope message)
            {
                Order retval = null;

                var order = message.Body.GetPayload<Order>();

                if (order.IsCompleted)
                    retval = order;

                return retval;
            }

            /// <summary>
            /// This is the point where all of the messages that 
            /// have been accumulated based on the aggregator 
            /// strategy will be combined into one message payload
            /// for processing.
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            public override IEnvelope DoAggregationStrategy(IEnvelope message)
            {
                // create a shipping invoice from the list of completed orders:
                var si = new ShippingInvoice();

                foreach (var item in base.GetAccumulatedOutput())
                {
                    si.AddOrder(item);
                }

                // keep all of the message header information (if necessary):
                var envelope = new Envelope(si) {Header = message.Header};
                return envelope;
            }
        }
    }
}