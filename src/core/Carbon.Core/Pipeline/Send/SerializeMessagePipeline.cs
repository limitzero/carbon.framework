using Carbon.Core.Builder;
using Carbon.Core.Pipeline.Component;

namespace Carbon.Core.Pipeline.Send
{
    /// <summary>
    /// Default pipeline for serializing a message.
    /// </summary>
    public class SerializeMessagePipeline : AbstractSendPipeline,
                                            IOnDemandPipelineConfiguration
    {
        public SerializeMessagePipeline(IObjectBuilder objectBuilder)
        {
            Name = "Serialize Message Pipeline";
            RegisterComponents(new SerializeMessagePipelineComponent(objectBuilder));
        }
    }
}