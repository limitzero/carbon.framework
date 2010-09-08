using System.Reflection;
using Carbon.Core.Builder;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Gateway.Impl
{
    public interface IGatewayProxyBuilder
    {
        /// <summary>
        /// This will build a proxy for the interface representing the gateway and register the 
        /// proxy instance in the container based on the definition.
        /// </summary>
        /// <param name="builder"><seealso cref="IObjectBuilder"/></param>
        /// <param name="gatewayDefinition">Definition of the gateway</param>
        void BuildProxy(IObjectBuilder builder, IGatewayDefinition gatewayDefinition);

        /// <summary>
        /// This will scan an assebly by name for all gateways that can be registered for use.
        /// </summary>
        /// <param name="builder"><seealso cref="IObjectBuilder"/></param>
        /// <param name="assemblyName">Name of the assembly to scan.</param>
        void Scan(IObjectBuilder builder, string assemblyName);

        /// <summary>
        /// This will scan an assembly for all adapters that can be registered for use.
        /// </summary>
        /// <param name="builder"><seealso cref="IObjectBuilder"/></param>
        /// <param name="assembly">Assembly to scan</param>
        void Scan(IObjectBuilder builder, Assembly assembly);

    }
}