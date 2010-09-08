using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Carbon.Core.Pipeline.Component;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send.Exceptions;

namespace Carbon.Core.Pipeline.Send
{
    /// <summary>
    /// Base implementation of a receive pipeline.
    /// </summary>
    public abstract class AbstractSendPipeline : ISendPipeline
    {
        private List<AbstractPipelineComponent> m_pipeline_components = null;

        public EventHandler<SendPipelineStartedEventArgs> SendPipelineStarted;
        public EventHandler<SendPipelineComponentInvokedEventArgs> SendPipelineComponentInvoked;
        public EventHandler<SendPipelineCompletedEventArgs> SendPipelineCompleted;
        public EventHandler<SendPipelineErrorEventArgs> SendPipelineError;
       
        public string Name { get; set; }

        /// <summary>
        /// (Read-Only). The collection of registered components that will be used to 
        ///  process the message before submission.
        /// </summary>
        public ReadOnlyCollection<AbstractPipelineComponent> PipelineComponents { get; private set; }

        protected AbstractSendPipeline()
        {
            m_pipeline_components = new List<AbstractPipelineComponent>();
        }

        public IEnvelope Invoke(PipelineDirection direction, IEnvelope envelope)
        {

            if (direction == PipelineDirection.Send)
                envelope = this.InvokePipeline(envelope);

            if (direction == PipelineDirection.Receive)
                throw new NotImplementedException();

            return envelope;
        }

        public void RegisterComponents(params AbstractPipelineComponent[] components)
        {
            foreach (var component in components)
            {
                if (!m_pipeline_components.Contains(component))
                    m_pipeline_components.Add(component);
            }
            this.PipelineComponents = m_pipeline_components.AsReadOnly();
        }

        public void RegisterComponents(params object[] components)
        {
            foreach (var component in components)
            {
                if (typeof(AbstractPipelineComponent).IsAssignableFrom(component.GetType()))
                    RegisterComponents(component as AbstractPipelineComponent);
            }
        }

        private IEnvelope InvokePipeline(IEnvelope envelope)
        {
            AbstractPipelineComponent currentPipelineComponent = null; 

            try
            {
                OnPipelineStarted(envelope);

                if (PipelineComponents.Count > 0)
                {
                    foreach (var pipelineComponent in PipelineComponents)
                    {
                        currentPipelineComponent = pipelineComponent;
                        envelope = currentPipelineComponent.Execute(envelope);

                        if (envelope is NullEnvelope)
                            throw new Exception(
                                string.Format(
                                    "Send pipeline '{0}' aborted due to null envelope message being returned from component '{1}-{2}'.",
                                    this.Name,
                                    currentPipelineComponent.Name,
                                    currentPipelineComponent.GetType().FullName));

                        OnPipelineComponentInvoked(currentPipelineComponent, envelope);
                    }
                }

                OnPipelineCompleted(envelope);
            }
            catch (Exception exception)
            {
                if (!OnPipelineError(currentPipelineComponent, envelope, exception))
                    throw new SendPipelineException(this, currentPipelineComponent, envelope, exception);
            }

            return envelope;
        }

        private void OnPipelineStarted(IEnvelope message)
        {
            EventHandler<SendPipelineStartedEventArgs> evt = this.SendPipelineStarted;
            if (evt != null)
                evt(this, new SendPipelineStartedEventArgs(this, message));
        }

        private void OnPipelineComponentInvoked(AbstractPipelineComponent component, IEnvelope envelope)
        {
            EventHandler<SendPipelineComponentInvokedEventArgs> evt = this.SendPipelineComponentInvoked;
            if (evt != null)
                evt(this, new SendPipelineComponentInvokedEventArgs(component, envelope));
        }

        private void OnPipelineCompleted(IEnvelope message)
        {
            EventHandler<SendPipelineCompletedEventArgs> evt = this.SendPipelineCompleted;
            if (evt != null)
                evt(this, new SendPipelineCompletedEventArgs(this, message));
        }

        private bool OnPipelineError(AbstractPipelineComponent component,  IEnvelope message, Exception exception)
        {
            EventHandler<SendPipelineErrorEventArgs> evt = this.SendPipelineError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new SendPipelineErrorEventArgs(this, message, component, exception));

            return isHandlerAttached;
        }
    }
}