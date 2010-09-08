using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Msmq.Adapter.Registration
{
    public class MsmqAdapterRegistrationStrategy : IAdapterRegistrationStrategy
    {
        public IObjectBuilder ObjectBuilder { get; set; }

        public IAdapterConfiguration Configure()
        {
            return new MsmqAdapterConfiguration().Configure(ObjectBuilder);
        }
    }
}