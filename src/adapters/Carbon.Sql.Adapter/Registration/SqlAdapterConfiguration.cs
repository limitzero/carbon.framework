using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Sql.Adapter.Registration
{
    public class SqlAdapterConfiguration : IAdapterConfiguration
    {
        public string Scheme { get; private set; }
        public string Uri { get; private set; }
        public Type InputChannelAdapter { get; private set; }
        public Type OutputChannelAdapter { get; private set; }

        public IAdapterConfiguration Configure(IObjectBuilder builder)
        {
            this.Scheme = "sql";
            this.Uri = "sql://{component id to connection information}";
            this.InputChannelAdapter = typeof (SqlInputChannelAdapter);
            this.OutputChannelAdapter = typeof(SqlOutputChannelAdapter);
            return this;
        }
    }
}