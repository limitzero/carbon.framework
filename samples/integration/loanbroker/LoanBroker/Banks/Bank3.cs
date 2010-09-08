using Carbon.Core.Channel.Template;

namespace LoanBroker.Banks
{
    public class Bank3 : AbstractBank
    {
        public Bank3(IChannelMessagingTemplate template) : 
            base(template)
        {
            Name = "Pawn Shop";
            PrimeRate = 5.5;
            RatePremium = 0.45;
            MaxLoanTerm = 10;
        }

        public override bool CanHandleLoanQuoteRequest(int CreditScore, double LoanAmount, int HistoryLength)
        {
            return LoanAmount >= 1000 && CreditScore >= 500 && HistoryLength >= 3;;
        }

    }
}