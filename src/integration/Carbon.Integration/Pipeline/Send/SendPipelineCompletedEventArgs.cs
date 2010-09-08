using System;
using Carbon.Core;

namespace Carbon.Integration.Pipeline.Send
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