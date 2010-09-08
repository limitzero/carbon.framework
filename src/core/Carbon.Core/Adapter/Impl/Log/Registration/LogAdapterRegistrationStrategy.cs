using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Impl.Log.Registration
{
    public class LogAdapterRegistrationStrategy : IAdapterRegistrationStrategy
    {
        public IObjectBuilder ObjectBuilder { get; set; }

        public IAdapterConfiguration Configure()
        {
            return new LogAdapterConfiguration().Configure(this.ObjectBuilder);
        }
    }
}