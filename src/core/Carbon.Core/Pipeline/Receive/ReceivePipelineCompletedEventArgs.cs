using System;

namespace Carbon.Core.Pipeline.Receive
{
    public class ReceivePipelineCompletedEventArgs : EventArgs
    {
        public AbstractReceivePipeline Pipeline { get; set; }
        public IEnvelope Envelope { get; set; }

        public ReceivePipelineCompletedEventArgs(AbstractReceivePipeline pipeline, IEnvelope envelope)
        {
            Pipeline = pipeline;
            Envelope = envelope;
        }
    }
}