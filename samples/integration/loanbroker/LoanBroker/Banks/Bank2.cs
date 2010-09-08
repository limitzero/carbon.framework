using Carbon.Core.Channel.Template;

namespace LoanBroker.Banks
{
    public class Bank2 : AbstractBank
    {
        public Bank2(IChannelMessagingTemplate template) :
            base(template)
        {
            // setup the bank information:
            Name = "Greater Trust Bank";
            PrimeRate = 4.5;
            RatePremium = 0.55;
            MaxLoanTerm = 72;
        }

        public override bool CanHandleLoanQuoteRequest(int CreditScore, double LoanAmount, int HistoryLength)
        {
            return LoanAmount >= 65000 && CreditScore >= 530 && HistoryLength >= 8;
        }

    }
}