using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Component.Adapter.Registration
{
    public class ComponentAdapterConfiguration : IAdapterConfiguration
    {
        public string Scheme { get; private set; }
        public string Uri { get; private set; }
        public Type InputChannelAdapter { get; private set; }
        public Type OutputChannelAdapter { get; private set; }

        public IAdapterConfiguration Configure(IObjectBuilder builder)
        {
            this.Scheme = "direct";
            this.Uri =
                "direct://{component id}/?method={component method name}&channel={channel where the message will be sent or received}";
            this.InputChannelAdapter = typeof (ComponentInputAdapter);
            this.OutputChannelAdapter = typeof(ComponentOutputAdapter);
            return this;
        }
    }
}