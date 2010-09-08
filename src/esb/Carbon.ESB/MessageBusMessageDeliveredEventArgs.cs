using System;

namespace Carbon.ESB
{
    public class MessageBusMessageDeliveredEventArgs : EventArgs
    {
        public string Location { get; private set; }
        public object Message { get; private set; }

        public MessageBusMessageDeliveredEventArgs(string location, object message)
        {
            Location = location;
            Message = message;
        }
    }
}