using System;
using Carbon.Core;
using Carbon.Integration.Pipeline.Send;

namespace Carbon.Integration.Pipeline.Receive
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