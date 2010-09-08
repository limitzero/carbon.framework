using Carbon.Core.Stereotypes.For.Components.Message;
namespace Starbucks.Messages
{
    [Message]
    public class PaymentNeededForOrderMessage
    {
        public decimal Amount { get; set; }
    }
}