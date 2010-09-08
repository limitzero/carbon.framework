using System;
using System.Collections.Generic;
using System.Threading;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Builder;
using Carbon.Integration.Testing;
using Starbucks.Surfaces.Barrista.Components;
using Xunit;
using Starbucks.Messages;
using Carbon.Integration.Stereotypes.Splitter;
using Carbon.Integration.Stereotypes.Router;
using Carbon.Integration.Stereotypes.Gateway;
using Carbon.Integration.Dsl.Surface;
using Carbon.Integration.Stereotypes.Accumulator;
using Carbon.Integration.Dsl.Surface.Registry;
using Carbon.Integration.Stereotypes.Consumes;

namespace Starbucks.Tests.Surfaces.Barrista
{
    public class when_the_barrista_receives_a_message_to_prepare_the_order : BaseMessageConsumerTestFixture
    {
        private ManualResetEvent _wait = null;
        private string _barrista_channel = "barrista";
        private string _cashier_channel = "cashier";
        private string _customer_channel = "customer";

        public when_the_barrista_receives_a_message_to_prepare_the_order()
            : base(@"empty.config.xml")
        {
            // setup the barrista message consumer component to accept messages on the "barrista" channel:
            CreateChannels(_barrista_channel, _cashier_channel, _customer_channel);
            RegisterComponent<BarristaMessageConsumer>(_barrista_channel);

            // send the message:
            _wait = new ManualResetEvent(false);

            var message = new Envelope(new PrepareOrderMessage());
            Context.Send(_barrista_channel, message);

            _wait.WaitOne(TimeSpan.FromSeconds(5));
            _wait.Set();
        }

        [Fact]
        public void it_will_create_the_order_for_the_customer_as_sent_by_the_cashier_and_send_the_completed_order_to_the_customer()
        {
            // make sure that the consumer is processing the correct input message:
            Assert.Equal(typeof(PrepareOrderMessage), MessageConsumerInputMessage.GetType());

            // after the order is completed, the "order prepared" message should be sent to the cashier:
            var orderPreparedMessage = ReceiveMessageFromChannel<OrderPreparedMessage>(_customer_channel, null);
            Assert.Equal(typeof(OrderPreparedMessage), orderPreparedMessage.GetType());
        }

    }


    public class SampleStarbucksTests
        : BaseMessageConsumerTestFixture
    {
        public SampleStarbucksTests()
            : base(@"empty.config.xml")
        {
            Context.LoadSurface<Starbucks>();
            Context.Start();
        }

        [Fact]
        public void can_place_an_order_and_receive_the_drinks_from_the_barrista_when_the_order_is_completed()
        {
            var gateway = Context.GetComponent<Cashier>();

            var surface = Context.GetComponent<ISurfaceRegistry>().Find<Starbucks>();
            surface.Configure();
            Console.WriteLine(surface.Verbalize());

            for (var index = 1; index < 2; index++)
            {
                var order = new CreateDrinkOrderMessage();
                order.AddDrink("MOCHA", DrinkSize.Small, true, 3);
                order.AddDrink("LATTE", DrinkSize.Medium, true, 2);
                gateway.PlaceOrder(order);
            }

            var wait = new ManualResetEvent(false);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();
        }    
    }

    public interface Cashier
    {
        [Gateway("orders")]
        void PlaceOrder(CreateDrinkOrderMessage message);
    }

    /// <summary>
    /// The drink order splitter will take a the drink order message
    /// and return a series of drinks for preparation.
    /// </summary>
    public class DrinkOrderSplitter 
        : ICanConsumeAndReturn<CreateDrinkOrderMessage, List<Drink>>
    {
        [Splitter]
        public List<Drink> Consume(CreateDrinkOrderMessage message)
        {
            // just return the lising of drinks on the order
            // one-by-one on the output channel:
            return message.Drinks;
        }
    }

    /// <summary>
    /// The drinks router will determine how the barrista
    /// will prepare the drinks for the customer.
    /// </summary>
    public class DrinkRouter
        : ICanConsumeAndReturn<Drink, string>
    {
        [Router]
        public string Consume(Drink message)
        {
            var route = message.IsIced == true ? "cold_drink" : "hot_drink";
            return route;
        }
    }

    /// <summary>
    /// Barrista that is dedicated to preparing drinks for the customer. 
    /// This will use the component channel adapter to process one 
    /// message to two methods of the component that will perform 
    /// differently.
    /// </summary>
    public class Barrista
    {
        public int HotDrinkDelay { get; set; }
        public int ColdDrinkDelay { get; set; }

        [Consumes("customer")]
        public Drink PrepareHotDrink(Drink message)
        {
            Console.WriteLine("Preparing hot drink...");
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(HotDrinkDelay));
            Console.WriteLine("Hot drink prepared.");

            var drink = new Drink(message.Name, message.Size, message.IsIced);
            return drink;
        }

        [Consumes("customer")]
        public Drink PrepareColdDrink(Drink message)
        {
            Console.WriteLine("Preparing cold drink...");
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ColdDrinkDelay));
            Console.WriteLine("Cold drink prepared.");

            var drink = new Drink(message.Name, message.Size, message.IsIced);
            return drink;
        }
    }


    public class Customer: ICanConsume<Drink>
    {
        private readonly IAdapterMessagingTemplate _template;

        public Customer(IAdapterMessagingTemplate template)
        {
            _template = template;
        }

        public void Consume(Drink message)
        {
            var msg = "Enjoying my " + message.Size.ToString() + " " + message.Name;
            _template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }
    }

    public class Starbucks : 
        AbstractIntegrationComponentSurface
    {
        public Starbucks(IObjectBuilder builder) : base(builder)
        {
            Name = "Starbucks";
            IsAvailable = true;
        }

        public override void BuildReceivePorts()
        {
           
        }

        public override void BuildCollaborations()
        {
            AddGateway<Cashier>("PlaceOrder");
            AddComponent<DrinkOrderSplitter>("orders", "drinks");
            AddComponent<DrinkRouter>("drinks");
            AddComponent<Barrista>("cold_drink", "customer", "PrepareColdDrink");
            AddComponent<Barrista>("hot_drink", "customer", "PrepareHotDrink");
            AddComponent<Customer>("customer");
        }

        public override void BuildSendPorts()
        {
            CreateSendPort(null, "hot_drink", "direct://Barrista/?method=PrepareColdDrink&channel=customer", 2, 1);
            CreateSendPort(null, "cold_drink", "direct://Barrista/?method=PrepareColdDrink&channel=customer", 2, 1);
        }

        public override void BuildErrorPort()
        {
            
        }
    }
}