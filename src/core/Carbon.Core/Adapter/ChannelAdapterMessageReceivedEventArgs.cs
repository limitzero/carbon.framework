using System;

namespace Carbon.Core.Adapter
{
    public class ChannelAdapterMessageReceivedEventArgs : EventArgs
    {
        public IEnvelope Envelope { get; private set; }
        public string Channel { get; private set; }
        public string Uri { get; private set; }

        public ChannelAdapterMessageReceivedEventArgs(IEnvelope envelope, string channel, string uri)
        {
            Envelope = envelope;
            Channel = channel;
            Uri = uri;
        }
    }
}