using System;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Integration.Testing;
using LoanBroker.Messages;
using LoanBroker.Surfaces.CreditBureau.Components;
using Xunit;
using Carbon.Core;

namespace LoanBroker.LoanQuoteEngine.Tests.MessageConsumers
{
    public class CreditBureauMessageConsumerTests : BaseMessageConsumerTestFixture
    {
        private string _inputChannel = string.Empty;
        private string _outputChannel = string.Empty;
        private Envelope _message = null;

        public CreditBureauMessageConsumerTests()
            :base(@"empty.config.xml")
        {
            _inputChannel = "credit_bureau_inquiry";
            _outputChannel = "credit_bureau_reply";
            _message = new Envelope(new CreditBureauInquiry());
           
            RegisterComponent<CreditBureauMessageConsumer>(_inputChannel, _outputChannel);       
        }

        [Fact]
        public void can_receive_a_request_for_a_credit_inquiry_and_return_back_a_credit_bureau_reply_message()
        {
            var wait = new ManualResetEvent(false);
            Context.GetComponent<IChannelRegistry>().FindChannel(_inputChannel).Send(_message);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // test to see if the message is delivered to the consumer:
            Assert.Equal(typeof(CreditBureauInquiry), MessageConsumerInputMessage.GetType());

            // make sure that the credit bureau request message is returned by the consumer:
            Assert.Equal(typeof(CreditBureauReply), MessageConsumerOutputMessage.GetType());
        }

    }
}