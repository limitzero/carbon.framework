using System;
using System.Collections.Generic;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Queue;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Configuration;
using Carbon.Integration.Stereotypes.Splitter;
using Carbon.Integration.Stereotypes.Splitter.Impl;
using Castle.Windsor;
using Xunit;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace Carbon.Integration.Tests.Stereotypes.Splitter
{
    public class SplitterTests
    {
        private IWindsorContainer _container = null;
        private IIntegrationContext _context = null;

        public SplitterTests()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            _container = new WindsorContainer(@"empty.config.xml");
            _container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            _context = _container.Resolve<IIntegrationContext>();

            // add the channels to the infrastructure:
            _context.GetComponent<IChannelRegistry>().RegisterChannel("number_splitter");
            _context.GetComponent<IChannelRegistry>().RegisterChannel("even_numbers");

            // register the component and create the message endpoint for it:
            _context.GetComponent<IObjectBuilder>().Register(typeof(NumberSplitter).Name, typeof(NumberSplitter));
            _context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint("number_splitter", string.Empty, string.Empty, new NumberSplitter());
        }

        [Fact]
        public void can_send_a_list_of_numbers_for_splitting_and_only_receive_odd_numbers_on_the_designated_channel()
        {
            // send the message over the channel to the endpoint via the message endpoint activator:
            var numbers = new List<int>();
            for (int i = 1; i <= 10; i++)
                numbers.Add(i); // 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 => 2, 4, 6, 8, 10

            _context.GetComponent<IChannelRegistry>().FindChannel("number_splitter").Send(new Envelope(numbers));

            // wait for the message to appear on the channel:
            var wait = new ManualResetEvent(true);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // need to find out how many even number messsages are on the channel (get concreate implementation):
            var evenNumberChannel = _context.GetComponent<IChannelRegistry>().FindChannel("even_numbers");
            var queue = evenNumberChannel as QueueChannel;
            Assert.Equal(5, queue.GetMessages().Count);
        }

        [Fact]
        public void can_send_a_list_of_numbers_for_splitting_and_return_the_entire_listing_if_no_strategy_is_applied()
        {
            // send the message over the channel to the endpoint via the message endpoint activator:
            var numbers = new List<string>();
            for (int i = 1; i <= 10; i++)
                numbers.Add(i.ToString());

            _context.GetComponent<IChannelRegistry>().RegisterChannel("numbers");
            //_context.GetComponent<IChannelRegistry>().FindChannel("number_splitter").Send(new Envelope(numbers));

            _context.GetComponent<IChannelRegistry>().FindChannel("number_splitter")
                .Send(new Envelope(new Numbers() {Items =  numbers}));

            // wait for the message to appear on the channel:
            var wait = new ManualResetEvent(true);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // need to find out how many even number messsages are on the channel (get concreate implementation):
            var numbersChannel = _context.GetComponent<IChannelRegistry>().FindChannel("numbers");
            var queue = numbersChannel as QueueChannel;
            Assert.Equal(10, queue.GetMessages().Count);
        }
    }

    [Message]
    public class Numbers
    {
       public List<string> Items { get; set; }
    }

    [MessageEndpoint("number_splitter")]
    public class NumberSplitter
    {
       [Splitter(typeof(EvenNumberSplitterStrategy), "even_numbers")]
        public List<int> SplitNumbers(List<int> numbers)
        {
            // this will take the collection of numbers 
            // and send the even numbers to the "even_numbers" 
            // channel:
            return numbers;
        }

        [Splitter("numbers")]
        public List<string> SplitStrings(List<string> strings)
        {
            return strings;
        }

        [Splitter("numbers")]
        public List<string> SplitStrings(Numbers message)
        {
            return message.Items;
        }
    }

    public class EvenNumberSplitterStrategy : AbstractSplitterMessageHandlingStrategy
    {
        public EvenNumberSplitterStrategy()
        {
            // make sure to check the output behavior of the channel:
            IsDuplicatesAllowedOnOutputChannel = false;
        }

        public override void DoSplitterStrategy(IEnvelope message)
        {
            // payload must be of type IEnumerable<T> in order for the 
            // message splitting to process correctly:
            var payload = message.Body.GetPayload<List<int>>();

            foreach (var item in payload)
            {
                if (item % 2 == 0)
                {
                    var envelope = new Envelope(item);
                    envelope.Header = message.Header;
                    OnMessageSplit(envelope);
                }
            }

        }
    }
}