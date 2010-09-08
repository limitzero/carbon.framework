using System;
using System.Collections;
using System.Collections.Generic;
using Carbon.Core.Pipeline.Component;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send;
using Castle.Core.Configuration;
using Castle.MicroKernel;

namespace Carbon.Integration.Configuration.Surface.Pipeline
{
    /// <summary>
    /// Builds and registers the pipeline components for the send or receive pipeline for a input or output port.
    /// </summary>
    public class PipelineBuilder
    {
        private readonly IKernel m_kernel;
        private IConfiguration m_configuration;

        public PipelineBuilder(IKernel kernel, IConfiguration configuration)
        {
            m_kernel = kernel;
            m_configuration = configuration;
        }

        public AbstractReceivePipeline BuildReceivePipeline()
        {
            // no "pipeline" element, return a flow through default pipeline:
            if(m_configuration.Children.Count == 0)
                m_configuration = null; 

            var pipelineElement = FindPipelineElement();
            if(pipelineElement == null)
            {
                var pipeline = new ReceivePipeline() {Name = "Flow Through"};
                pipeline.RegisterComponents(m_kernel.Resolve<FlowThroughPipelineComponent>());
                return pipeline;
            }

            // find the custom pipeline components from the configuration:
            var components = this.FindPipelineComponents(pipelineElement);

            // build the receive pipeline:
            var name = pipelineElement.Attributes["name"];
            var receivePipeline = new ReceivePipeline() {Name = name};
            receivePipeline.RegisterComponents(components);

            return receivePipeline;
        }

        public AbstractSendPipeline BuildSendPipeline()
        {
            // no "pipeline" element, return a flow through default pipeline:
            if (m_configuration.Children.Count == 0)
                m_configuration = null; 

            var pipelineElement = FindPipelineElement();
            if (pipelineElement == null)
            {
                var pipeline = new SendPipeline() {Name = "Flow Through"};
                pipeline.RegisterComponents(m_kernel.Resolve<FlowThroughPipelineComponent>());
                return pipeline;
            }

            // find the custom pipeline components from the configuration:
            var components = this.FindPipelineComponents(pipelineElement);

            // build the receive pipeline:
            var name = pipelineElement.Attributes["name"];
            var receivePipeline = new SendPipeline() { Name = name };
            receivePipeline.RegisterComponents(components);

            return receivePipeline;
        }

        private IConfiguration FindPipelineElement()
        {
            IConfiguration pipelineElement = null;

            if(m_configuration == null) return null;

            if (m_configuration.Name.Trim().ToLower() == "pipeline") return m_configuration;

            for(var index = 0; index < m_configuration.Children.Count; index++)
            {
                var element = m_configuration.Children[index];
                if(element.Name.Trim().ToLower() != "pipeline") continue;
                pipelineElement = element;
                break;
            }

            return pipelineElement;
        }

        private object[] FindPipelineComponents(IConfiguration pipelineElement)
        {
            var types = new List<object>();
            var componentsElement = pipelineElement.Children[0];

            if (componentsElement == null) return types.ToArray();

            for (var index = 0; index < pipelineElement.Children.Count; index++ )
            {
                var component = pipelineElement.Children[index];
                if(component.Name.Trim().ToLower() != "component") continue;

                var reference = component.Attributes["ref"];

                if(string.IsNullOrEmpty(reference)) continue;

                try
                {
                    var instance = m_kernel.Resolve(reference, new Hashtable());
                    if(!types.Contains(instance))
                        types.Add(instance);
                }
                catch (Exception exception)
                {
                    // component not found for reference:
                    throw;
                }

            }

            return types.ToArray(); 
        }
    }
}