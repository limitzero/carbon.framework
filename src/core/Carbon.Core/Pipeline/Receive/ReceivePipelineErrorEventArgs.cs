using System;

namespace Carbon.Core.Pipeline.Receive
{
    public class ReceivePipelineErrorEventArgs : EventArgs
    {
        public AbstractReceivePipeline Pipeline { get; set; }
        public IEnvelope Envelope { get; set; }
        public Exception Exception { get; set; }

        public ReceivePipelineErrorEventArgs(AbstractReceivePipeline pipeline, IEnvelope envelope, Exception exception)
        {
            Pipeline = pipeline;
            Envelope = envelope;
            Exception = exception;
        }
    }
}