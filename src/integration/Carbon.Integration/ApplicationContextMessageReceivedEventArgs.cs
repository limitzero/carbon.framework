using System;

namespace Carbon.Integration
{
    public class ApplicationContextMessageReceivedEventArgs : EventArgs
    {
        public object Message { get; private set; }

        public ApplicationContextMessageReceivedEventArgs(object message)
        {
            Message = message;
        }
    }
}