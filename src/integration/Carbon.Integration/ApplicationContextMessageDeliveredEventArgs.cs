using System;

namespace Carbon.Integration
{
    public class ApplicationContextMessageDeliveredEventArgs : EventArgs
    {
        public string Location { get; private set; }
        public object Message { get; private set; }

        public ApplicationContextMessageDeliveredEventArgs(string location, object message)
        {
            Location = location;
            Message = message;
        }
    }
}