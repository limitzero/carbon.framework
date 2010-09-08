using System;
using Carbon.Core.Builder;
using Carbon.Sql.Adapter.ContextProvider;

namespace Carbon.Sql.Adapter
{
    public class SqlAdapterUtils
    {
        static SqlAdapterUtils()
        {
        }

        public static ISqlContextProvider CreateContextProvider(IObjectBuilder context, string uri)
        {
            ISqlContextProvider provider = null;

            var channelUri = new Uri(uri);
            var contextProvider = context.Resolve(channelUri.Host);

            if (contextProvider != null)
                provider = contextProvider as ISqlContextProvider;

            return provider;
        }
    }
}