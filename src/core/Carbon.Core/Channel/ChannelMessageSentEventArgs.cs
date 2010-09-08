using System;

namespace Carbon.Core.Channel
{
    public class ChannelMessageSentEventArgs : EventArgs
    {
        public IEnvelope Envelope { get; private set; }
        public string Channel { get; private set; }

        public ChannelMessageSentEventArgs(IEnvelope envelope, string channel)
        {
            Envelope = envelope;
            Channel = channel;
        }
    }
}