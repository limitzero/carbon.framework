using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Builder;

namespace Carbon.Core.Pipeline.Component
{
    /// <summary>
    /// Basic flow through component that allows the message to pass unaltered.
    /// </summary>
    public class FlowThroughPipelineComponent : AbstractPipelineComponent
    {
        public FlowThroughPipelineComponent(IObjectBuilder objectBuilder) : base(objectBuilder)
        {
            Name = "Flow-through Pipeline Component";
        }

        public override IEnvelope Execute(IEnvelope envelope)
        {
            return envelope;
        }
    }
}
