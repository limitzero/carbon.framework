using System;
using Carbon.Core.Channel.Template;

namespace LoanBroker.Banks
{
    public class Bank1 : AbstractBank
    {
        public Bank1(IChannelMessagingTemplate template) : 
            base(template)
        {       
            // setup the bank information:
            Name = "Community Bank";
            PrimeRate = 4.5;
            RatePremium = 0.35;
            MaxLoanTerm = 60;
        }

        public override bool CanHandleLoanQuoteRequest(int CreditScore, double LoanAmount, int HistoryLength)
        {
            return LoanAmount >= 45000 && CreditScore >= 545 && HistoryLength >= 10;
        }
      
    }
}