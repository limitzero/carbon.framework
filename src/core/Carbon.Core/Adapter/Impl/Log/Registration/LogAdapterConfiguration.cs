using System;
using Carbon.Core.Adapter.Impl.Null;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Impl.Log.Registration
{
    public class LogAdapterConfiguration : IAdapterConfiguration
    {
        public string Scheme { get; private set; }
        public string Uri { get; private set; }
        public Type InputChannelAdapter { get; private set; }
        public Type OutputChannelAdapter { get; private set; }

        public IAdapterConfiguration Configure(IObjectBuilder builder)
        {
            this.Scheme = "log";
            this.Uri = "log://{logger name}/?level={debug, info, error, warn, fatal}";
            this.InputChannelAdapter = typeof (NullInputChannelAdapter);
            this.OutputChannelAdapter = typeof(LogOutputAdapter);
            return this;
        }
    }
}