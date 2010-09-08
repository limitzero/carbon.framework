using System;

namespace Carbon.ESB
{
    public class MessageBusErrorEventArgs : EventArgs
    {
        public object Message { get; private set; }
        public Exception Exception { get; private set; }

        public MessageBusErrorEventArgs(Exception exception)
            :this(null, exception)
        {
            
        }
        public MessageBusErrorEventArgs(object message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}