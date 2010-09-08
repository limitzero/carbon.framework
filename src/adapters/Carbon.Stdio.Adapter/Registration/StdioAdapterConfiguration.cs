using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Stdio.Adapter.Registration
{
    public class StdioAdapterConfiguration : IAdapterConfiguration
    {
        public string Scheme { get; private set; }
        public string Uri { get; private set; }
        public Type InputChannelAdapter { get; private set; }
        public Type OutputChannelAdapter { get; private set; }

        public IAdapterConfiguration Configure(IObjectBuilder builder)
        {
            this.Scheme = "stdio";
            this.Uri = "stdio://{unique name of console location}";
            this.InputChannelAdapter = typeof (StdioInputChannelAdapter);
            this.OutputChannelAdapter = typeof(StdioOutputChannelAdapter);
            return this;
        }
    }
}