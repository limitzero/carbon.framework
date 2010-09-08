using System;
using Carbon.Core.Pipeline.Component;

namespace Carbon.Core.Pipeline.Send
{
    public class SendPipelineComponentInvokedEventArgs : EventArgs
    {
        public AbstractPipelineComponent PipelineComponent { get; private set; }
        public IEnvelope Envelope { get; private set; }

        public SendPipelineComponentInvokedEventArgs(AbstractPipelineComponent component, IEnvelope envelope)
        {
            PipelineComponent = component;
            Envelope = envelope;
        }
    }
}