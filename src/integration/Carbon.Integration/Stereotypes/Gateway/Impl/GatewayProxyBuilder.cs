using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Reflection;
using System.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace Carbon.Integration.Stereotypes.Gateway.Impl
{
    /// This will create a proxy for the given interface and register a 
    /// method interceptor for processing the call on the interface.
    /// </summary>
    public class GatewayProxyBuilder : IGatewayProxyBuilder
    {
        /// <summary>
        /// This will build a proxy for the interface representing the gateway and register the 
        /// proxy instance in the container based on the definition.
        /// </summary>
        /// <param name="container"><seealso cref="IContainer"/></param>
        /// <param name="gatewayDefinition">Definition of the gateway</param>
        public void BuildProxy(IObjectBuilder container, IGatewayDefinition gatewayDefinition)
        {
            var methodInterceptor = new GatewayMethodInterceptor(container, gatewayDefinition);
            var gatewayInstance = container.Resolve<IReflection>().GetTypeForNamedInstance(gatewayDefinition.Contract);
            var proxy = BuildFor(gatewayInstance, methodInterceptor);
            container.Register(gatewayDefinition.Id, gatewayInstance, proxy);
        }

        /// <summary>
        /// This will scan an assebly by name for all gateways that can be registered for use.
        /// </summary>
        /// <param name="container"><seealso cref="IContainer"/></param>
        /// <param name="assemblyName">Name of the assembly to scan.</param>
        public void Scan(IObjectBuilder container,  string assemblyName)
        {
            this.Scan(container, Assembly.Load(assemblyName));
        }

        /// <summary>
        /// This will scan an assembly for all adapters that can be registered for use.
        /// </summary>
        /// <param name="container"><seealso cref="IContainer"/></param>
        /// <param name="assembly">Assembly to scan</param>
        public void Scan(IObjectBuilder container, Assembly assembly)
        {
            var definitions = this.FindAllGatewaysAndBuildDefinition(assembly);

            foreach (var definition in definitions)
                this.BuildProxy(container, definition);
        }

        private object BuildFor(Type gatewayInterfaceType, IInterceptor interceptor)
        {
            var generator = new ProxyGenerator();
            var proxy = generator.CreateInterfaceProxyWithoutTarget(gatewayInterfaceType, interceptor);
            return proxy;
        }

        private IGatewayDefinition[] FindAllGatewaysAndBuildDefinition(Assembly assembly)
        {
            var definitions = new List<IGatewayDefinition>();

            var interfaces = from type in assembly.GetTypes()
                             where type.IsInterface
                             select type;

            foreach (var contract in interfaces)
            {
                foreach (var method in contract.GetMethods())
                {
                    if (method.GetCustomAttributes(typeof(GatewayAttribute), true).Length > 0)
                    {

                        var attribute =
                            method.GetCustomAttributes(typeof(GatewayAttribute), true)[0] as
                            GatewayAttribute;

                        var definition = new GatewayDefinition()
                                             {
                                                 Id = contract.Name,
                                                 Contract = contract.AssemblyQualifiedName,
                                                 Method = method.Name,
                                                 RequestChannel = attribute.RequestChannel,
                                                 ReplyChannel =
                                                     attribute.ReplyChannel != string.Empty
                                                         ? attribute.ReplyChannel
                                                         : string.Empty,
                                                 ReceiveTimeout =
                                                     attribute.ReplyTimeout != 0 ? attribute.ReplyTimeout : 0
                                             };

                        if (!definitions.Contains(definition))
                            definitions.Add(definition);
                    }
                }

            }

            return definitions.ToArray();
        }
    }
}