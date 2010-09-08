using System;
using Carbon.Core;

namespace Carbon.Integration.Pipeline.Send
{
    public class SendPipelineErrorEventArgs : EventArgs
    {
        public AbstractSendPipeline Pipeline { get; set; }
        public IEnvelope Envelope { get; set; }
        public Exception Exception { get; set; }

        public SendPipelineErrorEventArgs(AbstractSendPipeline pipeline, IEnvelope envelope, Exception exception)
        {
            Pipeline = pipeline;
            Envelope = envelope;
            Exception = exception;
        }
    }
}