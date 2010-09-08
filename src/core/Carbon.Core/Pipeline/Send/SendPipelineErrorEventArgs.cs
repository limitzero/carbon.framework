using System;
using Carbon.Core.Pipeline.Component;

namespace Carbon.Core.Pipeline.Send
{
    public class SendPipelineErrorEventArgs : EventArgs
    {
        public AbstractSendPipeline Pipeline { get; private set; }
        public AbstractPipelineComponent PipelineComponent { get; private set; }
        public IEnvelope Envelope { get; private set; }
        public Exception Exception { get; private set; }

        public SendPipelineErrorEventArgs(AbstractSendPipeline pipeline, IEnvelope envelope, AbstractPipelineComponent component, Exception exception)
        {
            Pipeline = pipeline;
            PipelineComponent = component;
            Envelope = envelope;
            Exception = exception;
        }
    }
}