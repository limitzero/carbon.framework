using System;

namespace Carbon.Core.Adapter
{
    public class ChannelAdapterStoppedEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public ChannelAdapterStoppedEventArgs(string message)
        {
            Message = message;
        }
    }
}