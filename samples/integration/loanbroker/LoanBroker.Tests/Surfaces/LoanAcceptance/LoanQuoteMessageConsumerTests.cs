using System;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Integration.Testing;
using LoanBroker.Messages;
using LoanBroker.Surfaces.LoanAcceptance.Components;
using Xunit;

namespace LoanBroker.Tests.Surfaces.LoanAcceptance
{
    public class LoanQuoteMessageConsumerTests : BaseMessageConsumerTestFixture
    {
        private Envelope _message = null;
        private string _input_channel = string.Empty;
        private string _output_channel = string.Empty; 

        public static object _received_message = null;

        public LoanQuoteMessageConsumerTests()
            : base(@"empty.config.xml")
        {
            _input_channel = "new_loan";
            _output_channel = "credit_bureau_inquiry";
            _message = new Envelope(new LoanQuoteQuery());
            RegisterComponent<LoanQuoteMessageConsumer>(_input_channel, _output_channel);
        }

        [Fact]
        public void  can_translate_the_loan_quote_query_to_a_message_for_a_credit_bureau_inquiry_and_forward_the_message_to_credit_inquirey_channel()
        {
            var wait = new ManualResetEvent(false);

            Context.GetComponent<IChannelRegistry>().FindChannel(_input_channel).Send(_message);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // test to see if the message is delivered to the consumer:
            Assert.Equal(typeof(LoanQuoteQuery), MessageConsumerInputMessage.GetType());

            // check the output channel to see if the messge was placed there:
            var outputMsg = Context.GetComponent<IChannelRegistry>().FindChannel(_output_channel).Receive();
            Assert.NotEqual(typeof(NullEnvelope), outputMsg.GetType());
            Assert.Equal(typeof(CreditBureauInquiry), outputMsg.Body.GetPayload<object>().GetType());
        }
    }
}