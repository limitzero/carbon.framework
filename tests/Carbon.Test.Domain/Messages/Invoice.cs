using Carbon.Core.Stereotypes.For.Components.Message;

namespace Carbon.Test.Domain.Messages
{
    [Message]
    public class Invoice
    {
        public string Id { get; set; }
        public decimal Total { get; set; }
    }
}