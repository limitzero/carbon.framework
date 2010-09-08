using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Carbon.Core;
using Carbon.Core.Pipeline;
using Carbon.Integration.Pipeline.Component;

namespace Carbon.Integration.Pipeline.Send
{
    /// <summary>
    /// Base implementation of a send pipeline.
    /// </summary>
    public abstract class AbstractSendPipeline : IReceivePipeline
    {
        private List<AbstractPipelineComponent> m_pipeline_components = null;

        public EventHandler<SendPipelineStartedEventArgs> SendPipelineStarted;
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
            EventHandler<SendPipelineStartedEventArgs> evt = this.SendPipelineStarted;
            if (evt != null)
                evt(this, new SendPipelineStartedEventArgs(this, message));
        }

        private void OnPipelineCompleted(IEnvelope message)
        {
            EventHandler<SendPipelineCompletedEventArgs> evt = this.SendPipelineCompleted;
            if (evt != null)
                evt(this, new SendPipelineCompletedEventArgs(this, message));
        }

        private bool OnPipelineError(IEnvelope message, Exception exception)
        {
            EventHandler<SendPipelineErrorEventArgs> evt = this.SendPipelineError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new SendPipelineErrorEventArgs(this, message, exception));

            return isHandlerAttached;
        }
    }
}