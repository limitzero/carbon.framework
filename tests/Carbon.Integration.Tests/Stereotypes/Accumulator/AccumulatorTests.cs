using System;
using System.Collections.Generic;
using System.Threading;
using Carbon.Core;
using Carbon.Integration.Stereotypes.Accumulator;
using Carbon.Integration.Stereotypes.Accumulator.Impl;
using Carbon.Integration.Testing;
using Carbon.Core.Stereotypes.For.Components.Message;
using Xunit;

namespace Carbon.Integration.Tests.Stereotypes.Accumulator
{
    public class AccumulatorTests : BaseMessageConsumerTestFixture
    {
        public AccumulatorTests()
        {
            CreateChannels("in", "out");
            RegisterComponent<StockQuoteAccumulator>("in", "out");
        }

        [Fact]
        public void can_send_messages_to_the_accumulator_and_retreive_a_list_of_the_objects_on_the_output_channel()
        {

            Context.Send("in",
                new Envelope(new StockQuote() { Id = 1 }),
                new Envelope(new StockQuote() { Id = 2 }),
                new Envelope(new StockQuote() { Id = 3 }),
                new Envelope(new StockQuote() { Id = 4 }),
                new Envelope(new StockQuote() { Id = 5 }));

            var wait = new ManualResetEvent(false);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            var messages = ReceiveMessageFromChannel<StockQuote[]>("out", null);
            Assert.Equal(typeof(StockQuote[]), messages.GetType());
            Assert.Equal(5, messages.Length);
        }

        [Fact]
        public void can_send_messages_to_the_accumulator_and_retreive_a_list_of_the_objects_on_the_output_channel_after_10_seconds()
        {

            Context.Send("in",
                new Envelope(new OrderItem() { Id = 1 }),
                new Envelope(new OrderItem() { Id = 2 }),
                new Envelope(new OrderItem() { Id = 3 }),
                new Envelope(new OrderItem() { Id = 4 }),
                new Envelope(new OrderItem() { Id = 5 }));

            var wait = new ManualResetEvent(false);
            wait.WaitOne(TimeSpan.FromSeconds(15)); 
            wait.Set();

            var messages = ReceiveMessageFromChannel<OrderItem[]>("out", null);
            Assert.Equal(typeof(OrderItem[]), messages.GetType());
            Assert.Equal(5, messages.Length);
        }

    }

    [Message]
    public class StockQuote
    {
        public int Id { get; set; }
    }

    [Message]
    public class OrderItem
    {
        public int Id { get; set; }
    }

    public class StockQuoteAccumulator
        : ICanConsume<StockQuote>, 
          ICanConsume<OrderItem>
    {
        //[Accumulator(typeof(DefaultAccumulatorMessageHandlingStrategy<StockQuote>), true, 5)]
        [Accumulate(typeof(StockQuote), true, 5)]
        public void Consume(StockQuote message)
        {

        }

        [Accumulate(typeof(OrderItem),true, 0, true, "00:00:00:10")]
        public void Consume(OrderItem message)
        {
            throw new NotImplementedException();
        }
    }

    // custom accumulator strategy
    public class StockQuoteAccumulatorStrategy
        : AbstractAccumulatorMessageHandlingStrategy<StockQuote>
    {
        private static IList<StockQuote> _items;

        public StockQuoteAccumulatorStrategy()
        {
            if (_items == null)
                _items = new List<StockQuote>();
            SetStorage(_items);

            // clean up any objects:
            CleanUpAction = () => { _items = null; };
        }
    }


}