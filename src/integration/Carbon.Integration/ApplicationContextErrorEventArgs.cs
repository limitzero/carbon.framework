using System;

namespace Carbon.Integration
{
    public class ApplicationContextErrorEventArgs : EventArgs
    {
        public object Message { get; private set; }
        public Exception Exception { get; private set; }

        public ApplicationContextErrorEventArgs(Exception exception)
            :this(null, exception)
        {
            
        }
        public ApplicationContextErrorEventArgs(object message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}