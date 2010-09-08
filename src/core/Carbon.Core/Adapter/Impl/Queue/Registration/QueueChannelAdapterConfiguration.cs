using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Impl.Queue.Registration
{
    public class QueueChannelAdapterConfiguration : IAdapterConfiguration
    {
        public string Scheme { get; private set; }
        public string Uri { get; private set; }
        public Type InputChannelAdapter { get; private set; }
        public Type OutputChannelAdapter { get; private set; }

        public IAdapterConfiguration Configure(IObjectBuilder builder)
        {
            this.Scheme = "vm";
            this.Uri = "vm://{channel name}";
            this.InputChannelAdapter = typeof (QueueChannelInputAdapter);
            this.OutputChannelAdapter = typeof(QueueChannelOutputAdapter);
            return this;
        }
    }
}