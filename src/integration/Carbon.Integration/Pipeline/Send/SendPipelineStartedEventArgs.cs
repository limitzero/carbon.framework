using System;
using Carbon.Core;

namespace Carbon.Integration.Pipeline.Send
{
    public class SendPipelineStartedEventArgs : EventArgs
    {
        public AbstractSendPipeline Pipeline { get; set; }
        public IEnvelope Envelope { get; set; }

        public SendPipelineStartedEventArgs(AbstractSendPipeline pipeline, IEnvelope envelope)
        {
            Pipeline = pipeline;
            Envelope = envelope;
        }
    }

}