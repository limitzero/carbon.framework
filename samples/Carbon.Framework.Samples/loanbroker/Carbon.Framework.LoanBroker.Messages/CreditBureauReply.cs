using Carbon.Core.Stereotypes.For.Components.Message;

namespace LoanBroker.Messages
{
    [Message]
    public class CreditBureauReply
    {
        public int SSN { get; set; }
    }
}