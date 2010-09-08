using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;
using Carbon.Channel.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Receive.Exceptions;
using Carbon.Core.RuntimeServices;
using Carbon.Core.Pipeline;
using Carbon.Core.Adapter.Template;

namespace Carbon.Core.Adapter
{
    /// <summary>
    /// Abstract class for taking (i.e. "consuming") messages from a physical location 
    /// and loading them into a channel for processing.  This adapter can be either polled 
    /// or scheduled to inspect the physical location on a periodic basis.
    /// </summary>
    public abstract class AbstractInputChannelAdapter :
        AbstractBackgroundService, IInputChannelAdapter
    {
        private bool m_is_busy = false;
        private ManualResetEvent m_message_received_and_processed_event = new ManualResetEvent(false);
        private List<EventWaitHandle> m_wait_handles = new List<EventWaitHandle>();
        private IEnvelope m_current_message = new NullEnvelope();
        private object m_queue_lock = new object();
        private Queue<IEnvelope> m_queue = new Queue<IEnvelope>();
        private List<Action<ChannelAdapterMessageReceivedEventArgs>> m_actions_on_message_receipt;

        /// <summary>
        /// (Read-Only). The channel where the message will either be unloaded to 
        /// a storage location or uploaded from a storage location.
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// (Read-Write). The uri configuration used by the adapter to access the physical 
        /// location where messages will be stored or retrieved.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// (Read-Write). Flag to indicate whether the adapter supports transactions.
        /// </summary>
        public bool IsTransactional { get; set; }

        /// <summary>
        /// (Read-Only). Instance of the <seealso cref="IObjectBuilder">object builder</seealso> holding all application resources.
        /// </summary>
        public IObjectBuilder ObjectBuilder { get; private set; }

        /// <summary>
        /// (Read-Write). Pipeline used to pre-process the message after receipt from a location before loading it to a channel.
        /// </summary>
        public AbstractReceivePipeline Pipeline { get; set; }

        /// <summary>
        /// Event that is triggered when the channel adapter is started.
        /// </summary>
        public event EventHandler<ChannelAdapterStartedEventArgs> AdapterStarted;

        /// <summary>
        /// Event that is triggered when the channel adapter is started.
        /// </summary>
        public event EventHandler<ChannelAdapterStoppedEventArgs> AdapterStopped;

        /// <summary>
        /// Event that is triggered when the channel adapter encounters an error.
        /// </summary>
        public event EventHandler<ChannelAdapterErrorEventArgs> AdapterError;

        /// <summary>
        /// Event that is triggered when a message is picked up from the physical location and loaded into a channel.
        /// </summary>
        public event EventHandler<ChannelAdapterMessageReceivedEventArgs> AdapterMessageReceived;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="builder"></param>
        protected AbstractInputChannelAdapter(IObjectBuilder builder)
        {
            ObjectBuilder = builder;
            m_actions_on_message_receipt = new List<Action<ChannelAdapterMessageReceivedEventArgs>>();

            this.m_wait_handles.Add(m_message_received_and_processed_event);
        }

        public override void Start()
        {

            if (IsRunning)
                return;

            try
            {

                DoStartActivities();

                if (string.IsNullOrEmpty(this.ChannelName))
                    throw new ArgumentException("The name of the channel that will accept the message from the physical location can not be null or empty");

                this.BackgroundServiceError += Adapter_ServiceError;
                this.BackgroundServiceStarted += Adapter_ServiceStarted;
                this.BackgroundServiceStopped += Adapter_ServiceStopped;

                if (this.Pipeline != null)
                {
                    this.Pipeline.ReceivePipelineCompleted += PipelineCompleted;
                    this.Pipeline.ReceivePipelineComponentInvoked += PipelineComponentInvoked;
                    this.Pipeline.ReceivePipelineStarted += PipelineStarted;
                }

                base.Start();
            }
            catch (Exception exception)
            {
                this.Stop();

                if (!OnAdapterError(exception))
                    throw;
            }

        }

        public override void Stop()
        {
            try
            {
                if (this.Pipeline != null)
                {
                    this.Pipeline.ReceivePipelineCompleted -= PipelineCompleted;
                    this.Pipeline.ReceivePipelineComponentInvoked -= PipelineComponentInvoked;
                    this.Pipeline.ReceivePipelineStarted -= PipelineStarted;
                }

                DoStopActivities();
            }
            catch (Exception exception)
            {
                if (!OnAdapterError(exception))
                    throw;
            }
            finally
            {
                base.Stop();
            }

            this.BackgroundServiceError -= Adapter_ServiceError;
            this.BackgroundServiceStarted -= Adapter_ServiceStarted;
            this.BackgroundServiceStopped -= Adapter_ServiceStopped;
        }

        /// <summary>
        /// This will set the channel where the message will either be unloaded to a storage 
        /// location or uploaded from a storage location.
        /// </summary>
        /// <param name="channelName"></param>
        public void SetChannel(string channelName)
        {
            this.ChannelName = channelName;
        }

        /// <summary>
        /// This will set a complete messaging envelope that 
        /// can be used in the Receive() method.
        /// </summary>
        /// <param name="message"></param>
        public void SetMessageForReceive(IEnvelope message)
        {
            this.m_current_message = message;
        }

        /// <summary>
        /// This will be invoked in a periodic fashion for the 
        /// custom service code to perform some actions specific 
        /// to their design.
        /// </summary>
        public override void PerformAction()
        {
            this.Receive();
        }

        /// <summary>
        /// This will return the scheme used by the adapter.
        /// </summary>
        public string GetScheme()
        {
            return new Uri(this.Uri).Scheme;
        }

        /// <summary>
        /// This will run the initial actions needed by the custom input adapter
        /// before it is started.
        /// </summary>
        public virtual void DoStartActivities()
        {
        }

        /// <summary>
        /// This will run the clean-up actions needed by the custom input adapter
        /// before it is stopped.
        /// </summary>
        public virtual void DoStopActivities()
        {
        }

        /// <summary>
        /// This will extract the message from the native storage location.
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ExtractMessageContents();

        /// <summary>
        /// This will create the header for the message that is retreived from the storage location
        /// supplying any information for identifying the message.
        /// </summary>
        /// <returns></returns>
        public abstract IEnvelopeHeader CreateMessageHeader();

        /// <summary>
        /// This is the extention point where all input adapter implemenations will extract a message from 
        /// their defined storage location by the appropriate technology. The tuple should return header 
        /// will all of the message charateristics defined and a byte array containing the contents of the 
        /// retreived message.
        /// </summary>
        /// <returns></returns>
        public abstract Tuple<IEnvelopeHeader, byte[]> DoReceive();

        /// <summary>
        /// This will periodically inspect the storage location and take the first available message 
        /// and place it on the channel for processing. 
        /// </summary>
        public IEnvelope Receive()
        {
            IEnvelope envelope = new NullEnvelope();

            //var handle = WaitHandle.WaitAny(this.m_wait_handles.ToArray());
            //var eventWaitHandle = this.m_wait_handles.ToArray()[handle];

            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    #region -- code that already works --

                    // delegate to the concrete implementation for receiving the message:
                    if (!(this.m_current_message is NullEnvelope))
                    {
                        envelope = m_current_message;
                    }
                    else
                    {
                        var tuple = this.DoReceive();

                        if(!(this.m_current_message is NullEnvelope))
                            envelope = this.m_current_message;
                        else
                        {
                            // no payload retrieved from the location, exit:
                            if (tuple.Item2 == null) return envelope;

                            envelope = new Envelope(tuple.Item2) { Header = tuple.Item1 };
                        }

                        if (Pipeline != null)
                            envelope = Pipeline.Invoke(PipelineDirection.Receive, envelope);
                    }

                    //all processing of the message will be done
                     //when this event is fired, any exceptions
                     //will rollback the transaction pushing the 
                     //message back to the persistance store:
                    OnAdapterMessageReceived(envelope);

                    transaction.Complete();

                    #endregion
                }
                catch (ReceivePipelineException rpe)
                {
                    if (!OnAdapterError(rpe))
                        throw;
                }
                catch (ThreadAbortException threadAbortException)
                {
                    // nothing to do here...wait for the next message.
                }
                catch (Exception exception)
                {
                    if (!OnAdapterError(exception))
                        throw;
                }
            }

            return envelope;
        }

        /// <summary>
        /// This will register a callback when the message is received on the adapter.
        /// </summary>
        /// <param name="action"></param>
        public void RegisterActionOnMessageReceived(Action<ChannelAdapterMessageReceivedEventArgs> action)
        {
            this.m_actions_on_message_receipt.Add(action);
        }

        private void ReceiveNextMessage()
        {
            m_current_message = new NullEnvelope();

            m_is_busy = true;

            // delegate to the concrete implementation for receiving the message:
            var tuple = this.DoReceive();

            // no payload retrieved from the location, exit:
            if (tuple.Item2 == null)
            {
                m_message_received_and_processed_event.Set();
            }

            m_current_message = new Envelope(tuple.Item2) { Header = tuple.Item1 };

            if (Pipeline != null)
                m_current_message = Pipeline.Invoke(PipelineDirection.Receive, m_current_message);

            m_message_received_and_processed_event.Set();
        }

        private void OnAdapterMessageReceived(IEnvelope envelope)
        {
            if (envelope is NullEnvelope) return;

            EventHandler<ChannelAdapterMessageReceivedEventArgs> evt = this.AdapterMessageReceived;

            // place the message on the channel (only if callbacks are not used):
            if (m_actions_on_message_receipt.Count == 0)
            {
                var channel = this.ObjectBuilder.Resolve<IChannelRegistry>().FindChannel(this.ChannelName);

                if (!(channel is NullChannel))
                    channel.Send(envelope);
            }

            // invoke the registered callbacks:
            m_actions_on_message_receipt.ForEach(x =>
                x.Invoke(new ChannelAdapterMessageReceivedEventArgs(envelope, this.ChannelName, this.Uri)));

            // invoke the event listner:
            if (evt != null)
                evt(this, new ChannelAdapterMessageReceivedEventArgs(envelope, this.ChannelName, this.Uri));
          
        }

        private bool OnAdapterError(Exception exception)
        {
            EventHandler<ChannelAdapterErrorEventArgs> evt = this.AdapterError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new ChannelAdapterErrorEventArgs(exception));

            return isHandlerAttached;
        }

        private void OnAdapterStarted()
        {
            EventHandler<ChannelAdapterStartedEventArgs> evt = this.AdapterStarted;
            if (evt != null)
                evt(this, new ChannelAdapterStartedEventArgs(string.Format("Input adapter for channel '{0}' on uri '{1}' started.", this.ChannelName, this.Uri)));
        }

        private void OnAdapterStopped()
        {
            EventHandler<ChannelAdapterStoppedEventArgs> evt = this.AdapterStopped;
            if (evt != null)
                evt(this, new ChannelAdapterStoppedEventArgs(string.Format("Input adapter for channel '{0}' on uri '{1}' stopped.", this.ChannelName, this.Uri)));
        }

        private void Adapter_ServiceStarted(object sender, BackGroundServiceEventArgs e)
        {
            OnAdapterStarted();
        }

        private void Adapter_ServiceStopped(object sender, BackGroundServiceEventArgs e)
        {
            OnAdapterStopped();
        }

        private void Adapter_ServiceError(object sender, BackGroundServiceErrorEventArgs e)
        {
            OnAdapterError(e.Exception);
        }

        private void PipelineStarted(object sender, ReceivePipelineStartedEventArgs e)
        {
            var template = ObjectBuilder.Resolve<IAdapterMessagingTemplate>();
            if (template == null) return;

            var pipeline = sender as AbstractReceivePipeline;
            var msg = string.Format("Receive pipeline '{0}' for channel '{1}' on uri '{2}' started.", pipeline.Name,
                                    this.ChannelName, this.Uri);
            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }

        private void PipelineComponentInvoked(object sender, ReceivePipelineComponentInvokedEventArgs e)
        {
            var template = ObjectBuilder.Resolve<IAdapterMessagingTemplate>();
            if (template == null) return;

            var pipeline = sender as AbstractReceivePipeline;
            var msg = string.Format("Receive pipeline '{0}' component '{1}' invoked for channel '{2}' on uri '{3}' with message '{4}'.",
                pipeline.Name,
                e.PipelineComponent.Name,
                this.ChannelName,
                this.Uri,
                e.Envelope.Body.GetPayload<object>().GetType().FullName);

            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }

        private void PipelineCompleted(object sender, ReceivePipelineCompletedEventArgs e)
        {
            var template = ObjectBuilder.Resolve<IAdapterMessagingTemplate>();
            if (template == null) return;

            var pipeline = sender as AbstractReceivePipeline;
            var msg = string.Format("Receive pipeline '{0}' for channel '{1}' on uri '{2}' completed.", pipeline.Name,
                                    this.ChannelName, this.Uri);
            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }
    }
}