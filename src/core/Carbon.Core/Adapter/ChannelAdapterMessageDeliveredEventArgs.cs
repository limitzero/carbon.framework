using System;

namespace Carbon.Core.Adapter
{
    public class ChannelAdapterMessageDeliveredEventArgs : EventArgs
    {
        public IEnvelope Envelope { get; private set; }
        public string Channel { get; private set; }
        public string Uri { get; private set; }

        public ChannelAdapterMessageDeliveredEventArgs(IEnvelope envelope, string channel, string uri)
        {
            Envelope = envelope;
            Channel = channel;
            Uri = uri;
        }
    }
}