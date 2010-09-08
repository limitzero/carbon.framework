using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Msmq.Adapter.Registration
{
    public class MsmqAdapterConfiguration : IAdapterConfiguration
    {
        public string Scheme { get; private set; }
        public string Uri { get; private set; }
        public Type InputChannelAdapter { get; private set; }
        public Type OutputChannelAdapter { get; private set; }

        public IAdapterConfiguration Configure(IObjectBuilder builder)
        {
            this.Scheme = "msmq";
            this.Uri = "msmq://{server name}/private$/{queue name}";
            this.InputChannelAdapter = typeof (MsmqInputAdapter);
            this.OutputChannelAdapter = typeof(MsmqOutputAdapter);
            return this;
        }
    }
}