using System;
using Carbon.Core.Pipeline.Component;

namespace Carbon.Core.Pipeline.Receive
{
    public class ReceivePipelineComponentInvokedEventArgs : EventArgs
    {
        public AbstractPipelineComponent PipelineComponent { get; private set; }
        public IEnvelope Envelope { get; private set; }

        public ReceivePipelineComponentInvokedEventArgs(AbstractPipelineComponent component,   IEnvelope envelope)
        {
            PipelineComponent = component;
            Envelope = envelope;
        }
    }
}