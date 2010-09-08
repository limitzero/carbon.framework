using Carbon.Core.Stereotypes.For.Components.Message;

namespace LoanBroker.Messages
{
    [Message]
    public class CreateBankQuoteMessage
    {
        public double LoanAmount { get; set; }
        public int LoanTerm { get; set; }
        public int SSN { get; set; }
        public int CreditScore { get; set; }
        public int HistoryLength { get; set; }
        public string ReplyTo { get; set; }
    }
}