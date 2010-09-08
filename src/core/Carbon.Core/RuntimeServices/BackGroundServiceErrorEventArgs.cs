using System;

namespace Carbon.Core.RuntimeServices
{
    public class BackGroundServiceErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        public BackGroundServiceErrorEventArgs()
        {

        }

        public BackGroundServiceErrorEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}