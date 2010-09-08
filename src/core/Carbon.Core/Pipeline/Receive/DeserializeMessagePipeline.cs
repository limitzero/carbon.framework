using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Builder;
using Carbon.Core.Pipeline.Component;

namespace Carbon.Core.Pipeline.Receive
{
    /// <summary>
    /// Default pipeline for deserializing a message.
    /// </summary>
    public class DeserializeMessagePipeline : AbstractReceivePipeline, 
        IOnDemandPipelineConfiguration
    {
        public DeserializeMessagePipeline(IObjectBuilder objectBuilder)
        {
            Name = "Deserialize Message Pipeline";
            RegisterComponents(new DeserializeMessagePipelineComponent(objectBuilder));
        }
    }
}
