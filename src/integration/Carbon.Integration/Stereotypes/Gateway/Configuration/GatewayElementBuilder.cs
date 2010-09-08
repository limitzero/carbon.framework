using System;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Configuration;
using Carbon.Core.Internals.Reflection;
using Carbon.Integration.Stereotypes.Gateway.Impl;
using Castle.Core.Configuration;

namespace Carbon.Integration.Stereotypes.Gateway.Configuration
{
    public class GatewayElementBuilder : AbstractElementBuilder
    {
        private const string m_element_name = "gateway";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var id = configuration.Attributes["id"];
            var type = configuration.Attributes["type"];
            var method = configuration.Attributes["method"];
            var definition = new GatewayDefinition() { Id = id, Method = method, Contract = type };
            this.CreateProxyForInterfaceAndRegister(definition, null);
        }

        public void Build(Type currentGatewayContract, string methodName)
        {
            var id = string.Concat(currentGatewayContract.Name, "_", methodName);
            var type = currentGatewayContract.AssemblyQualifiedName;
            var method = methodName;
            var definition = new GatewayDefinition() { Id = id, Method = method, Contract = type };
            this.CreateProxyForInterfaceAndRegister(definition, null);
        }

        public void Build(IObjectBuilder builder, Type currentGatewayContract, string methodName)
        {
            var id = string.Concat(currentGatewayContract.Name, "_", methodName);
            var type = currentGatewayContract.AssemblyQualifiedName;
            var method = methodName;
            var definition = new GatewayDefinition() { Id = id, Method = method, Contract = type };
            this.CreateProxyForInterfaceAndRegister(definition, builder);
        }

        private void CreateProxyForInterfaceAndRegister(IGatewayDefinition definition, IObjectBuilder builder)
        {
            IReflection reflection = null;
            IObjectBuilder localBuilder = builder;

            if (localBuilder == null)
                reflection = Kernel.Resolve<IReflection>();
            else
            {
                reflection = builder.Resolve<IReflection>();
            }

            if (localBuilder == null)
                localBuilder = Kernel.Resolve<IObjectBuilder>();

            var gateway = reflection.GetTypeForNamedInstance(definition.Contract);

            if (gateway == null)
                return;

            // create the proxy and register in container:
            var config = this.RegisterChannels(localBuilder, gateway, definition.Method);

            definition.RequestChannel = config.Item1;
            definition.ReplyChannel = config.Item2;
            definition.ReceiveTimeout = config.Item3;

            new GatewayProxyBuilder().BuildProxy(localBuilder, definition);

            //var methodInterceptor = new GatewayMethodInterceptor(Kernel, definition);
            //var proxy = GatewayProxyBuilder.BuildFor(gateway, methodInterceptor);
            //Kernel.AddComponentInstance(definition.Id, gateway, proxy);
        }

        private Tuple<string, string, int> RegisterChannels(IObjectBuilder builder, Type gateway, string method)
        {
            var timeout = 0;
            var requestChannel = string.Empty;
            var replyChannel = string.Empty;

            var attributes = gateway.GetMethod(method).GetCustomAttributes(typeof(GatewayAttribute), true);

            if (attributes.Length > 0)
            {
                var attr = (GatewayAttribute)attributes[0];
                IChannelRegistry registry = null;

                if (builder != null)
                    registry = builder.Resolve<IChannelRegistry>();
                else
                {
                    registry = Kernel.Resolve<IChannelRegistry>();
                }

                if (!string.IsNullOrEmpty(attr.RequestChannel))
                {
                    requestChannel = attr.RequestChannel;
                    if (registry.FindChannel(attr.RequestChannel) is NullChannel)
                        registry.RegisterChannel(attr.RequestChannel);
                }

                if (!string.IsNullOrEmpty(attr.ReplyChannel))
                {
                    replyChannel = attr.ReplyChannel;
                    if (registry.FindChannel(attr.ReplyChannel) is NullChannel)
                        registry.RegisterChannel(attr.ReplyChannel);

                }

                if (attr.ReplyTimeout > 0 && !string.IsNullOrEmpty(attr.ReplyChannel))
                    timeout = attr.ReplyTimeout;
            }

            return new Tuple<string, string, int>(requestChannel, replyChannel, timeout);
        }
    }
}