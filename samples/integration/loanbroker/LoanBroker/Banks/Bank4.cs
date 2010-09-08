using Carbon.Core.Channel.Template;

namespace LoanBroker.Banks
{
    public class Bank4 : AbstractBank
    {
        public Bank4(IChannelMessagingTemplate template)
            : base(template)
        {
            // setup the bank information:
            Name = "Ivory Tower Bank";
            PrimeRate = 3.5;
            RatePremium = 0.45;
            MaxLoanTerm = 72;
        }

        public override bool CanHandleLoanQuoteRequest(int CreditScore, double LoanAmount, int HistoryLength)
        {
            return LoanAmount >= 95000 && CreditScore >= 645 && HistoryLength >= 12;
        }
    }
}