using System;
using System.Collections;
using System.Threading;
using Carbon.Core;
using Carbon.Integration.Testing;
using Castle.MicroKernel.Registration;
using LoanBroker.Banks;
using LoanBroker.Messages;
using Xunit;

namespace LoanBroker.Tests.Banks
{
    public class BankTests : BaseMessageConsumerTestFixture
    {
        private string _inputChannel = string.Empty;
        private Bank1 _test_bank;
        private string _outputChannel;

        public BankTests()
            : base(@"empty.config.xml")
        {
            _inputChannel = "community.bank";
            _outputChannel = "bank_quote_replies";

            var props = new Hashtable();
            props.Add("Name","Community Bank");
            props.Add("PrimeRate", 4.5);
            props.Add("RatePremium", 0.35);
            props.Add("MaxLoanTerm", 60);

            // manually add the component to the container with the dependent properties set:
            Container.Register(
                Component.For(typeof(Bank1))
                    .DependsOn(props).Named("community.bank"));

            // create all of the channels for the test:
            CreateChannels(_inputChannel, _outputChannel);

            _test_bank = RegisterComponentById<Bank1>("community.bank", _inputChannel, _outputChannel);
        }


        [Fact]
        public void Can_generate_a_bank_quote_over_the_input_channel_and_return_a_bank_quote_to_the_output_channel()
        {
            var wait = new ManualResetEvent(false);

            Context.Send(_inputChannel, new Envelope(new CreateBankQuoteMessage()));
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            var message = ReceiveMessageFromChannel<BankQuoteCreatedMessage>(_outputChannel, null);

            // assert that the message was delivered to the component:
            Assert.Equal(typeof(CreateBankQuoteMessage), MessageConsumerInputMessage.GetType());
            Assert.Equal(typeof(BankQuoteCreatedMessage), message.GetType());
        }

    }
}