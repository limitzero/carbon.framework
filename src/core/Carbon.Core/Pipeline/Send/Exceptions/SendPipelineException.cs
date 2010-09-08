using System;
using Carbon.Core.Pipeline.Component;

namespace Carbon.Core.Pipeline.Send.Exceptions
{
    public class SendPipelineException : ApplicationException
    {
        public AbstractSendPipeline Pipeline { get; private set; }
        public AbstractPipelineComponent PipelineComponent { get; private set; }
        public IEnvelope Envelope { get; private set; }
        public Exception Exception { get; private set; }

        private const string m_error_message =
            "An error has occurred for the send pipeline '{0}' on channel '{1}' for component '{2}'. Reason: {3}";

        public SendPipelineException(AbstractSendPipeline pipeline, AbstractPipelineComponent pipelineComponent, IEnvelope envelope, Exception exception)
            :base(string.Format(m_error_message, pipeline.Name, envelope.Header.InputChannel,  pipelineComponent.Name,  exception.ToString()))
        {
            Pipeline = pipeline;
            PipelineComponent = pipelineComponent;
            Envelope = envelope;
            Exception = exception;
        }
    }
}