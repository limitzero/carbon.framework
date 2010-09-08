using System;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Sql.Adapter.Registration
{
    public class SqlAdapterRegistrationStrategy : IAdapterRegistrationStrategy
    {
        public IObjectBuilder ObjectBuilder { get; set; }

        public IAdapterConfiguration Configure()
        {
            return new SqlAdapterConfiguration().Configure(ObjectBuilder);
        }
    }
}