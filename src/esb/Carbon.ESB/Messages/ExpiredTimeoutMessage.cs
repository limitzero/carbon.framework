using Carbon.Core.Stereotypes.For.Components.Message;

namespace Carbon.ESB.Messages
{
    [Message]
    public class ExpiredTimeOutMessage
    {
        public object Message { get; set; }
    }
}