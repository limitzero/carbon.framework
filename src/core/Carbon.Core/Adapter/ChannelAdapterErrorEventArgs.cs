using System;

namespace Carbon.Core.Adapter
{
    public class ChannelAdapterErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        public ChannelAdapterErrorEventArgs(Exception exception)
            :this(string.Empty, exception)
        {
        }

        public ChannelAdapterErrorEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}