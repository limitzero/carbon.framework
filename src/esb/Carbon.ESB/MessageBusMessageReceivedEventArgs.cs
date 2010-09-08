using System;

namespace Carbon.ESB
{
    public class MessageBusMessageReceivedEventArgs : EventArgs
    {
        public object Message { get; private set; }

        public MessageBusMessageReceivedEventArgs(object message)
        {
            Message = message;
        }
    }
}