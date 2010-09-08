using System;

namespace Carbon.Core.Channel
{
    public class ChannelMessageReceivedEventArgs : EventArgs
    {
        public IEnvelope Envelope { get; private set; }
        public string Channel { get; private set; }

        public ChannelMessageReceivedEventArgs(IEnvelope envelope, string channel)
        {
            Envelope = envelope;
            Channel = channel;
        }
    }
}