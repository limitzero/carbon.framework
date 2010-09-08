using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.File.Adapter.Registration
{
    public class FileAdapterConfiguration : IAdapterConfiguration
    {
        public string Scheme { get; private set; }
        public string Uri { get; private set; }
        public Type InputChannelAdapter { get; private set; }
        public Type OutputChannelAdapter { get; private set; }

        public IAdapterConfiguration Configure(IObjectBuilder builder)
        {
            this.Scheme = "file";
            this.Uri = "file://{location to directory}/?delete={true | false}";
            this.InputChannelAdapter = typeof (Carbon.File.Adapter.FileInputAdapter);
            this.OutputChannelAdapter = typeof(Carbon.File.Adapter.FileOutputAdapter);
            return this;
        }
    }
}