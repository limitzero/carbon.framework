using System;

namespace Carbon.Core.Pipeline.Receive
{
    public class ReceivePipelineStartedEventArgs : EventArgs
    {
        public AbstractReceivePipeline Pipeline { get; set; }
        public IEnvelope Envelope { get; set; }

        public ReceivePipelineStartedEventArgs(AbstractReceivePipeline pipeline, IEnvelope envelope)
        {
            Pipeline = pipeline;
            Envelope = envelope;
        }
    }
}