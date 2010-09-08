using System;

namespace Carbon.Core.Channel
{
    public class ChannelErrorEncounteredEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        public ChannelErrorEncounteredEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}