using System;
using System.Collections;
using Carbon.Channel.Registry;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Configuration;
using Carbon.Core.Stereotypes.For.Components.Service.Impl;
using Castle.Core.Configuration;

namespace Carbon.Core.Stereotypes.For.Components.Service.Configuration
{
    /// <summary>
    /// Element builder for defining a service activator:
    /// 
    /// Configuration:
    /// <![CDATA[
    /// <!-- service activator definition for component -->
    /// <service-activator
    ///   input-channel="in"
    ///   output-channel="out"
    ///   method="Echo"
    ///   ref="echo" />
    /// 
    /// <!-- component definitions in the container -->
    /// <components>
    ///  <component id="echo"
    ///    type="TestComponents.EchoService, TestComponents" />
    /// </components>
    /// ]]>
    /// 
    /// </summary>
    public class ServiceActivatorElementBuilder : AbstractElementBuilder
    {
        private const string m_element_name = "service-activator";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var inputChannel = configuration.Attributes["input-channel"];
            var outputChannel = configuration.Attributes["output-channel"];
            var reference = configuration.Attributes["ref"];
            var method = configuration.Attributes["method"];

            // find the instance for the reference:
            var instance = this.Kernel.Resolve(reference, new Hashtable());

            if(instance == null)
                return;

            this.BuildActivatorForService(inputChannel, outputChannel, instance.GetType());

        }

        private void BuildActivatorForService(string inputChannelName, 
                                              string outputChannelName, Type service)
        {
            // create the message dispatcher for the end point:
            var inputChannel = this.Kernel.Resolve<IChannelRegistry>().FindChannel(inputChannelName);

            if (inputChannel is NullChannel)
                return;

            var activator = this.Kernel.Resolve<IServiceActivator>();
            activator.SetInputChannel(inputChannelName);
            activator.SetOutputChannel(outputChannelName);
            activator.SetServiceInstance(service);

        }
    }
}