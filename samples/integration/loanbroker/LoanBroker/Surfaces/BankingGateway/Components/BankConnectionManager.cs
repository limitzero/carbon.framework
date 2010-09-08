using System;
using Carbon.Core;
using Carbon.Core.Channel.Template;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.BankingGateway.Components
{
    public class BankConnectionManager : IBankConnectionManager
    {
        private readonly IChannelMessagingTemplate _template;

        public string BankQuoteReplyAddress { get; set; }

        public string[] BankingPartners { get; set; }

        public BankConnectionManager(IChannelMessagingTemplate template)
        {
            _template = template;
        }

        public void Consume(CreditBureauReply message)
        {
            // forward the quote message to the participating banks
            // and ask them to send their reply to a common address:
            var quote = new CreateBankQuoteMessage()
                            {
                                SSN = message.SSN,
                                CreditScore = message.CreditScore,
                                HistoryLength =  message.HistoryLength,
                                LoanAmount = message.LoanAmount, 
                                LoanTerm = message.LoanTerm,
                                ReplyTo = BankQuoteReplyAddress
                            };

            foreach (var bank in BankingPartners)
                try
                {
                    _template.DoSend(bank, new Envelope(quote));
                }
                catch
                {
                    continue;
                }

        }

    }
}