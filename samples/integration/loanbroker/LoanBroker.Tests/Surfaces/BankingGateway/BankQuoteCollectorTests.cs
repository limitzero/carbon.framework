using System;
using System.Threading;
using Carbon.Core;
using Carbon.Integration.Testing;
using Castle.MicroKernel.Registration;
using LoanBroker.Banks;
using LoanBroker.Surfaces.BankingGateway.Components;
using Xunit;
using LoanBroker.Messages;
using System.Collections.Generic;

namespace LoanBroker.Tests.BankQuoteCollector
{
    public class BankQuoteMessageAccumulatorTests : BaseMessageConsumerTestFixture
    {
        private string _inputChannel = string.Empty;
        private string _outputChannel = string.Empty;

        public BankQuoteMessageAccumulatorTests()
            : base(@"empty.config.xml")
        {
            _inputChannel = "bank_quote_replies";
            _outputChannel = "bank_quote_aggregator";

            // create the channels for accepting the collection of bank quotes
            // and the channel where the final listing will be sent to:
            CreateChannels(_inputChannel,_outputChannel);

            // manually add the component to the container with the dependent properties set:
            Container.Register(
                Component.For(typeof(BankQuoteMessageAccumulator))
                    .Named("bank.quote.accumulator"));

            var component = RegisterComponentById<BankQuoteMessageAccumulator>("bank.quote.accumulator",
                _inputChannel, _outputChannel);
        }

        [Fact]
        public void can_accumulate_all_bank_quotes_issued_by_the_banks_for_sending_out_as_a_batch_on_the_aggregator_channel()
        {
            var message = new Envelope(new BankQuoteCreatedMessage());
            var messages = new Envelope[] {message, message, message, message};

            Context.Send(_inputChannel, messages);
               
            var wait = new ManualResetEvent(false);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            var receivedMessages = ReceiveMessageFromChannel<BankQuoteCreatedMessage[]>(_outputChannel, null);
            Assert.Equal(typeof(BankQuoteCreatedMessage[]), receivedMessages.GetType());
            Assert.Equal(messages.Length, receivedMessages.Length);
        }

    }
}