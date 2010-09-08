using System;

namespace Carbon.Core.Adapter
{
    public class ChannelAdapterStartedEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public ChannelAdapterStartedEventArgs(string message)
        {
            Message = message;
        }
    }
}