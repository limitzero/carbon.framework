using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Carbon.Core;
using Carbon.Core.Pipeline;
using Carbon.Integration.Pipeline.Component;

namespace Carbon.Integration.Pipeline.Receive
{
    /// <summary>
    /// Base implementation of a receive pipeline.
    /// </summary>
    public abstract class AbstractReceivePipeline : IReceivePipeline
    {
        private List<AbstractPipelineComponent> m_pipeline_components = null;

        public EventHandler<ReceivePipelineStartedEventArgs> ReceivePipelineStarted;
        public EventHandler<ReceivePipelineCompletedEventArgs> ReceivePipelineCompleted;
        public EventHandler<ReceivePipelineErrorEventArgs> ReceivePipelineError;

        public string Name { get; set; }

        /// <summary>
        /// (Read-Only). The collection of registered components that will be used to 
        ///  process the message after receipt.
        /// </summary>
        public ReadOnlyCollection<AbstractPipelineComponent> PipelineComponents { get; private set; }

        protected AbstractReceivePipeline()
        {
            m_pipeline_components = new List<AbstractPipelineComponent>();
        }

        public IEnvelope Invoke(PipelineDirection direction, IEnvelope envelope)
        {

            if (direction == PipelineDirection.Receive)
                envelope = this.InvokePipeline(envelope);

            if (direction == PipelineDirection.Send)
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

        private IEnvelope InvokePipeline(IEnvelope envelope)
        {
            try
            {
                OnPipelineStarted(envelope);

                foreach (var pipelineComponent in PipelineComponents)
                    envelope = pipelineComponent.Execute(envelope);

                OnPipelineCompleted(envelope);
            }
            catch (Exception exception)
            {
                if (!OnPipelineError(envelope, exception))
                    throw;
            }

            return envelope;
        }

        private void OnPipelineStarted(IEnvelope message)
        {
            EventHandler<ReceivePipelineStartedEventArgs> evt = this.ReceivePipelineStarted;
            if (evt != null)
                evt(this, new ReceivePipelineStartedEventArgs(this, message));
        }

        private void OnPipelineCompleted(IEnvelope message)
        {
            EventHandler<ReceivePipelineCompletedEventArgs> evt = this.ReceivePipelineCompleted;
            if (evt != null)
                evt(this, new ReceivePipelineCompletedEventArgs(this, message));
        }

        private bool OnPipelineError(IEnvelope message, Exception exception)
        {
            EventHandler<ReceivePipelineErrorEventArgs> evt = this.ReceivePipelineError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new ReceivePipelineErrorEventArgs(this, message, exception));

            return isHandlerAttached;
        }
    }
}