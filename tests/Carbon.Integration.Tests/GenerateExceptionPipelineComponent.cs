using System;
using Carbon.Core.Builder;
using Carbon.Core.Pipeline.Component;
namespace Carbon.Integration.Tests
{
    public class GenerateExceptionPipelineComponent : AbstractPipelineComponent
    {
        public GenerateExceptionPipelineComponent(IObjectBuilder objectBuilder)
            : base(objectBuilder)
        {
            Name = "Generate Exception Pipeline Component";
        }

        public override Carbon.Core.IEnvelope Execute(Carbon.Core.IEnvelope envelope)
        {
            throw new Exception("This is the exception from the exception generating pipeline component");
            return envelope;
        }
    }
}