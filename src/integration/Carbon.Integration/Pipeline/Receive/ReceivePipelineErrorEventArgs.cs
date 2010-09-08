using System;
using Carbon.Core;
using Carbon.Integration.Pipeline.Send;

namespace Carbon.Integration.Pipeline.Receive
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