using Carbon.Core.Stereotypes.For.Components.Message;

namespace LoanBroker.Messages
{
    [Message]
    public class CreditBureauInquiry
    {
        public double LoanAmount { get; set; }
        public int LoanTerm { get; set; }
        public int SSN { get; set; }
    }
}