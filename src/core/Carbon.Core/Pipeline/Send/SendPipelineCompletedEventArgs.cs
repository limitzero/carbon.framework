using System;

namespace Carbon.Core.Pipeline.Send
{
    public class SendPipelineCompletedEventArgs : EventArgs
    {
        public AbstractSendPipeline Pipeline { get; set; }
        public IEnvelope Envelope { get; set; }

        public SendPipelineCompletedEventArgs(AbstractSendPipeline pipeline, IEnvelope envelope)
        {
            Pipeline = pipeline;
            Envelope = envelope;
        }
    }
}