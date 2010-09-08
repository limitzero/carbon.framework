using System;

namespace Carbon.ESB
{
    public class MessageBusMessagePublishedEventArgs : EventArgs
    {
        public object Message { get; private set; }

        public MessageBusMessagePublishedEventArgs(object message)
        {
            Message = message;
        }
    }
}