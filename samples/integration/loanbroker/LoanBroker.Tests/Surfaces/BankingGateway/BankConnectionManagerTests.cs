using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Integration.Testing;
using Castle.MicroKernel.Registration;
using LoanBroker.Messages;
using LoanBroker.Surfaces.BankingGateway.Components;
using Xunit;

namespace LoanBroker.Tests.Surfaces.BankingGateway
{
    public class BankConnectionManagerTests : BaseMessageConsumerTestFixture
    {
        private IBankConnectionManager _connection_manager = null;
        private Envelope _message = null;
        private string _inputChannel;
        private List<string> _banks = null;

        public BankConnectionManagerTests()
            : base(@"empty.config.xml")
        {
            _inputChannel = "credit_buereau_reply";

            // listing of channels where the banks can be reached (asynchronous communication..later):
            _banks = new List<string>();
            _banks.Add("bank1");
            _banks.Add("bank2");

            var props = new Hashtable();
            props.Add("BankingPartners", _banks.ToArray());
            props.Add("BankQuoteReplyAddress", "bank_quote_replies");

            // manually add the component to the container with the dependent properties set:
            Container.Register(
                Component.For(typeof(BankConnectionManager))
                    .DependsOn(props).Named("bank.connection.manager"));

            _connection_manager = RegisterComponentById<BankConnectionManager>("bank.connection.manager", _inputChannel);

            // create all of the channels for the banks:
            foreach (var bank in _banks)
                this.Context.GetComponent<IChannelRegistry>().RegisterChannel(bank);

            // message to forward to bank connection manager:
            _message = new Envelope(new CreditBureauReply());
        }

        [Fact]
        public void Can_receive_credit_bureau_reply_message_and_route_the_messages_to_the_participating_banks__over_their_corresponding_channels()
        {
            var wait = new ManualResetEvent(false);

            Context.Send(_inputChannel, _message);
            
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // find out if the message got to the connection manager:
            Assert.Equal(typeof(CreditBureauReply), MessageConsumerInputMessage.GetType());

            // the message should be forwarded to the channels listed above, check the channel 
            // locations to see if the message is there for consumption by the banks:
            var message1 = ReceiveMessageFromChannel<CreateBankQuoteMessage>(_banks[0], null);
            var message2 = ReceiveMessageFromChannel<CreateBankQuoteMessage>(_banks[1], null);

            // make sure each bank receives the message to generate a quote:
            Assert.Equal(typeof(CreateBankQuoteMessage), message1.GetType());
            Assert.Equal(typeof(CreateBankQuoteMessage), message2.GetType());

        }
    }
}