using System;
using Carbon.Channel.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Configuration;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.MessageHandling.Gateway.Impl;
using Castle.Core.Configuration;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Gateway.Configuration
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
            this.CreateProxyForInterfaceAndRegister(definition);
        }

        public void Build(Type currentGatewayContract, string methodName)
        {
            var id = string.Concat(currentGatewayContract.Name, "_", methodName);
            var type = currentGatewayContract.AssemblyQualifiedName;
            var method = methodName;
            var definition = new GatewayDefinition() { Id = id, Method = method, Contract = type};
            this.CreateProxyForInterfaceAndRegister(definition);
        }

        private void CreateProxyForInterfaceAndRegister(IGatewayDefinition definition)
        {
            var reflection = Kernel.Resolve<IReflection>();
            var gateway = reflection.GetTypeForNamedInstance(definition.Contract);

            if (gateway == null)
                return;

            // create the proxy and register in container:
            this.RegisterChannels(gateway, definition.Method);
            new GatewayProxyBuilder().BuildProxy(Kernel.Resolve<IObjectBuilder>(), definition);

            //var methodInterceptor = new GatewayMethodInterceptor(Kernel, definition);
            //var proxy = GatewayProxyBuilder.BuildFor(gateway, methodInterceptor);
            //Kernel.AddComponentInstance(definition.Id, gateway, proxy);
        }

        private void RegisterChannels(Type gateway, string method)
        {
            var attributes = gateway.GetMethod(method).GetCustomAttributes(typeof (GatewayAttribute), true);
            if(attributes.Length > 0)
            {
                var attr = (GatewayAttribute) attributes[0];

                if (!string.IsNullOrEmpty(attr.RequestChannel))
                    if (this.Kernel.Resolve<IChannelRegistry>().FindChannel(attr.RequestChannel) is NullChannel)
                        this.Kernel.Resolve<IChannelRegistry>().RegisterChannel(attr.RequestChannel);

                if (!string.IsNullOrEmpty(attr.ReplyChannel))
                    if (this.Kernel.Resolve<IChannelRegistry>().FindChannel(attr.ReplyChannel) is NullChannel)
                        this.Kernel.Resolve<IChannelRegistry>().RegisterChannel(attr.ReplyChannel);
            }
        }
    }
}