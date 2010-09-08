using System;
using Carbon.Component.Adapter.Registration;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Component.Adapter.Registration
{
    public class ComponentAdapterRegistrationStrategy : IAdapterRegistrationStrategy
    {
        public IObjectBuilder ObjectBuilder { get; set; }

        public IAdapterConfiguration Configure()
        {
            return new ComponentAdapterConfiguration().Configure(ObjectBuilder);
        }
    }
}