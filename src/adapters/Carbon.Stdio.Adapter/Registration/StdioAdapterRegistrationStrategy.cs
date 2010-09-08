using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Stdio.Adapter.Registration
{
    public class StdioAdapterRegistrationStrategy : IAdapterRegistrationStrategy
    {
        public IObjectBuilder ObjectBuilder { get; set; }

        public IAdapterConfiguration Configure()
        {
            return new StdioAdapterConfiguration().Configure(ObjectBuilder);
        }
    }
}