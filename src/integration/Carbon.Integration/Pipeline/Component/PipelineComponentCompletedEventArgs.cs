using System;
using Carbon.Core;

namespace Carbon.Integration.Pipeline.Component
{
    /// <summary>
    /// Event args sent when a pipeline component completes.
    /// </summary>
    public class PipelineComponentCompletedEventArgs : EventArgs
    {
        public AbstractPipelineComponent Component { get; set; }
        public IEnvelope Message { get; set; }

        public PipelineComponentCompletedEventArgs(AbstractPipelineComponent component, IEnvelope message)
        {
            Component = component;
            Message = message;
        }
    }
}