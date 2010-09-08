using System;
using Carbon.Core;

namespace Carbon.Integration.Pipeline.Component
{
    /// <summary>
    /// Event args sent when a pipeline component starts.
    /// </summary>
    public class PipelineComponentStartedEventArgs : EventArgs
    {
        public AbstractPipelineComponent Component { get; set; }
        public IEnvelope Message { get; set; }

        public PipelineComponentStartedEventArgs(AbstractPipelineComponent component, IEnvelope message)
        {
            Component = component;
            Message = message;
        }
    }
}