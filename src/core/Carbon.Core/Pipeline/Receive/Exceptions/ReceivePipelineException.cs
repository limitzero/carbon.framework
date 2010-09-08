using System;

namespace Carbon.Core.Pipeline.Receive.Exceptions
{
    public class ReceivePipelineException : ApplicationException
    {
        private const string m_error_message =
            "An error has occurred for the receive pipeline '{0}' on channel '{1}'. Reason: {2}";

        public ReceivePipelineException(AbstractReceivePipeline pipeline, IEnvelope envelope, Exception exception)
            :base(string.Format(m_error_message, pipeline.Name, envelope.Header.InputChannel, exception.ToString()))
        {
            
        }
    }
}