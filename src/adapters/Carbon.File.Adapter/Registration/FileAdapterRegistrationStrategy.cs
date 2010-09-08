using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.File.Adapter.Registration
{
    public class FileChannelAdapterRegistrationStrategy : IAdapterRegistrationStrategy
    {
        public IObjectBuilder ObjectBuilder { get; set; }

        public IAdapterConfiguration Configure()
        {
            return new FileAdapterConfiguration().Configure(ObjectBuilder);
        }
    }
}