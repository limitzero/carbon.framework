using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Impl.Queue.Registration
{
    public class QueueChannelAdapterRegistrationStrategy : IAdapterRegistrationStrategy
    {
        public IObjectBuilder ObjectBuilder { get; set; }

        public IAdapterConfiguration Configure()
        {
            return new QueueChannelAdapterConfiguration().Configure(ObjectBuilder);
        }
    }
}