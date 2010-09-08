using System;
using System.Collections.Generic;
using System.Threading;
using Carbon.Core;
using Carbon.Integration.Testing;
using LoanBroker.Messages;
using LoanBroker.Surfaces.BankingGateway.Components;
using Xunit;

namespace LoanBroker.Tests.Surfaces.BankingGateway
{
    public class BankQuoteAggregatorTests : BaseMessageConsumerTestFixture
    {
        private string _input_channel;
        private string _output_channel;

        public BankQuoteAggregatorTests()
            :base(@"empty.config.xml")
        {
            _input_channel = "bank_accumulated_quotes";
            _output_channel = "bank_quote_translator";

            CreateChannels(_input_channel, _output_channel);

            RegisterComponent<BankQuoteMessageAggregator>(_input_channel, _output_channel);
        }

        [Fact]
        public void can_receive_a_list_of_bank_quotes_and_determine_the_best_one_according_to_the_lowest_interest_rate()
        {
            // send in the list of messages:
            var messages = this.GetMessages();
            Context.Send(_input_channel, new Envelope(messages));

            var wait = new ManualResetEvent(false);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            var quote = ReceiveMessageFromChannel<BankQuoteCreatedMessage>(_output_channel, null);
            Assert.NotNull(quote);
            Assert.Equal(1, quote.InterestRate); // lowest rate should be here!!!
        }

        private BankQuoteCreatedMessage[] GetMessages()
        {
            var messages = new List<BankQuoteCreatedMessage>();
            for (var index = 1; index < 5; index++)
            {
                var message = new BankQuoteCreatedMessage()
                                  {ErrorCode = 0, InterestRate = index, QuoteId = index.ToString()};
                messages.Add(message);
            }

            return messages.ToArray();
        }
    }
}