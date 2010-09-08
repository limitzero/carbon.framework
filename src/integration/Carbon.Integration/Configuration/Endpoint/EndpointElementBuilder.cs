using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Configuration;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Castle.Core.Configuration;
using Carbon.Channel.Registry;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Registries.For.MessageEndpoints;

namespace Carbon.Integration.Configuration.Endpoint
{
    public class EndpointElementBuilder : AbstractElementBuilder
    {
        private const string m_element_name = "endpoint";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name.Trim().ToLower();
        }

        public override void Build(IConfiguration configuration)
        {
            string minimalConfigurationForElement =
                "<endpoint input-channel={some name - required} ref={reference to object in container - required} />";

            var inputChannel = configuration.Attributes["input-channel"];
            var outputChannel = configuration.Attributes["output-channel"];
            var reference = configuration.Attributes["ref"];
            var method = configuration.Attributes["method"];

            if(string.IsNullOrEmpty(inputChannel))
                throw new Exception("The input channel for the endpoint can not be blank or empty. Please specify an input channel name for the endpoint where messages will be forwarded.");

            if (string.IsNullOrEmpty(reference))
                throw new Exception("The reference (ref) attribute on the endpoint element must refer to a component instance for a class registered in the container.");

            // find the instance for the reference:
            var instance = this.Kernel.Resolve(reference, new Hashtable());

            if (instance == null)
                return;

            this.BuildActivatorForEndpoint(inputChannel, outputChannel, method, instance);
        }

        private void BuildActivatorForEndpoint(string inputChannel, string outputChannel, string method, object endpoint)
        {
            if (Kernel.Resolve<IChannelRegistry>().FindChannel(inputChannel) is NullChannel)
                Kernel.Resolve<IChannelRegistry>().RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                if (Kernel.Resolve<IChannelRegistry>().FindChannel(outputChannel) is NullChannel)
                    Kernel.Resolve<IChannelRegistry>().RegisterChannel(outputChannel);

            var activator = Kernel.Resolve<IMessageEndpointActivator>();
            activator.ActivationStyle = EndpointActivationStyle.ActivateOnMessageReceived;
            activator.SetInputChannel(inputChannel);
            activator.SetOutputChannel(outputChannel);
            
            if(!string.IsNullOrEmpty(method))
                activator.SetEndpointInstanceMethodName(method);

            activator.SetEndpointInstance(endpoint);

            Kernel.Resolve<IMessageEndpointRegistry>().Register(activator);
        }
    }
}
