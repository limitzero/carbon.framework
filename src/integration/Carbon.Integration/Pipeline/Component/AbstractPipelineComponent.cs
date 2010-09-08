using System;
using Carbon.Core;
using Carbon.Core.Builder;

namespace Carbon.Integration.Pipeline.Component
{
    /// <summary>
    /// Base pipeline component that all components will derive from.
    /// </summary>
    public abstract class AbstractPipelineComponent : IPipelineComponent
    {
        public EventHandler<PipelineComponentStartedEventArgs> PipelineComponentStarted;
        public EventHandler<PipelineComponentCompletedEventArgs> PipelineComponentCompleted;
        public EventHandler<PipelineComponentErrorEventArgs> PipelineComponentError;

        public IObjectBuilder ObjectBuilder { get; set; }

        protected AbstractPipelineComponent(IObjectBuilder objectBuilder)
        {
            ObjectBuilder = objectBuilder;
        }

        public string Name { get; set; }

        public abstract IEnvelope Execute(IEnvelope envelope); 

        public void OnComponentStarted(AbstractPipelineComponent component, IEnvelope message)
        {
            EventHandler<PipelineComponentStartedEventArgs> evt = this.PipelineComponentStarted;
            if(evt != null)
                evt(component, new PipelineComponentStartedEventArgs(component, message));
        }

        public void OnComponentCompleted(AbstractPipelineComponent component, IEnvelope message)
        {
            EventHandler<PipelineComponentCompletedEventArgs> evt = this.PipelineComponentCompleted;
            if (evt != null)
                evt(component, new PipelineComponentCompletedEventArgs(component, message));
        }

        public bool OnComponentError(AbstractPipelineComponent component, IEnvelope message, Exception exception)
        {
            EventHandler<PipelineComponentErrorEventArgs> evt = this.PipelineComponentError;
            var isHandlerAttached = (evt != null);

            if(isHandlerAttached)
                evt(component, new PipelineComponentErrorEventArgs(component, message, exception));

            return isHandlerAttached;
        }
    }
}