using System;

namespace Carbon.Core.Pipeline.Component
{
    /// <summary>
    /// Event args sent when a pipeline component encounters an error.
    /// </summary>
    public class PipelineComponentErrorEventArgs : EventArgs
    {
        public AbstractPipelineComponent Component { get; set; }
        public IEnvelope Message { get; set; }
        public Exception Exception { get; set; }

        public PipelineComponentErrorEventArgs(AbstractPipelineComponent component, IEnvelope message, Exception exception)
        {
            Component = component;
            Message = message;
            Exception = exception;
        }
    }
}