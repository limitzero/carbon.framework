using System;
using System.Text;
using Carbon.Core.Adapter.Strategies.Retry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Channel.Impl.Queue;
using Carbon.Core.Exceptions;
using Carbon.Core.Pipeline.Send;
using Carbon.Core.Pipeline.Send.Exceptions;
using Carbon.Core.RuntimeServices;
using Carbon.Channel.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Pipeline;
using System.Transactions;
using Carbon.Core.Adapter.Impl.Queue;

namespace Carbon.Core.Adapter
{
    /// <summary>
    /// Abstract class for taking messages from a channel and loading them into a physical location for processing.
    /// </summary>
    public abstract class AbstractOutputChannelAdapter :
        AbstractBackgroundService, IOutputChannelAdapter
    {
        private IChannelRegistry m_channel_registry = null;

        /// <summary>
        /// (Read-Only). The channel where the message will either be unloaded to a storage location or uploaded from a storage location.
        /// </summary>
        public string ChannelName { get; private set; }

        /// <summary>
        /// (Read-Write). The uri configuration used by the adapter to access the physical location where messages will be stored or retrieved.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// (Read-Write). Flag to indicate whether the adapter supports transactions.
        /// </summary>
        public bool IsTransactional { get; set; }

        /// <summary>
        /// (Read-Write). The strategy used for retrying messages upon initial submission error.
        /// </summary>
        public IRetryStrategy RetryStrategy { get; set; }

        /// <summary>
        /// (Read-Only). Instance of the <seealso cref="IObjectBuilder">object builder</seealso> holding all application resources.
        /// </summary>
        public IObjectBuilder ObjectBuilder { get; private set; }

        /// <summary>
        /// (Read-Write). Pipeline used to pre-process the message before sending it to a location.
        /// </summary>
        public AbstractSendPipeline Pipeline { get; set; }

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
        /// Event that is triggered when the message is delivered to the storage location.
        /// </summary>
        public event EventHandler<ChannelAdapterMessageDeliveredEventArgs> AdapterMessgeDelivered;

        /// <summary>
        /// (Read-Write). Message that is returned for the send operation that can be used 
        /// for distributing to an input adapter if used in a synchronous fashion.
        /// </summary>
        public IEnvelope ResponseMessage { get; set; }

        protected AbstractOutputChannelAdapter(IObjectBuilder builder)
        {
            ObjectBuilder = builder;
        }

        public override void Start()
        {
            if (IsRunning)
                return;

            try
            {
                DoStartActivities();

                if (string.IsNullOrEmpty(this.ChannelName))
                    throw new ArgumentException("The name of the channel that will be examined for moving the payload to the physical location can not be null or empty");

                this.BackgroundServiceError += Adapter_ServiceError;
                this.BackgroundServiceStarted += Adapter_ServiceStarted;
                this.BackgroundServiceStopped += Adapter_ServiceStopped;

                if (this.Pipeline != null)
                {
                    this.Pipeline.SendPipelineCompleted += PipelineCompleted;
                    this.Pipeline.SendPipelineComponentInvoked += PipelineComponentInvoked;
                    this.Pipeline.SendPipelineStarted += PipelineStarted;
                    this.Pipeline.SendPipelineError += PipelineError;
                }

                base.Start();
            }
            catch (Exception exception)
            {
                if (!OnAdapterError(exception))
                    throw;
            }
            base.Start();
        }



        public override void Stop()
        {
            this.BackgroundServiceError -= Adapter_ServiceError;
            this.BackgroundServiceStarted -= Adapter_ServiceStarted;
            this.BackgroundServiceStopped -= Adapter_ServiceStopped;

            if (this.Pipeline != null)
            {
                this.Pipeline.SendPipelineCompleted -= PipelineCompleted;
                this.Pipeline.SendPipelineComponentInvoked -= PipelineComponentInvoked;
                this.Pipeline.SendPipelineStarted -= PipelineStarted;
                this.Pipeline.SendPipelineError -= PipelineError;
            }

            try
            {

                try
                {
                    ClearChannelMessages();
                }
                catch
                {
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
        }

        public override void PerformAction()
        {
            // constantly poll the channel for message to receive
            // and then send them to the location:
            var channel = this.ObjectBuilder.Resolve<IChannelRegistry>()
                .FindChannel(this.ChannelName);

            if (channel is NullChannel)
                return;

            IEnvelope envelope = new NullEnvelope(); 

            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    envelope = channel.Receive(1);

                    if (envelope is NullEnvelope)
                        return;

                    if (Pipeline != null)
                        envelope = this.Pipeline.Invoke(PipelineDirection.Send, envelope);

                    this.Send(envelope);
                    transaction.Complete();

                }
                catch (SendPipelineException spe)
                {
                    // push to error location (if defined);
                    if(!OnAdapterError(spe))
                        throw;
                }
                catch
                {
                    // no messages for receipt on channel:
                }
            }

        }

        /// <summary>
        /// This will return the scheme used by the adapter.
        /// </summary>
        public string GetScheme()
        {
            var uri = new Uri(this.Uri);
            return uri.Scheme;
        }

        /// <summary>
        /// This will set the channel where the message will either be unloaded to a storage location or uploaded from a storage location.
        /// </summary>
        /// <param name="channelName"></param>
        public void SetChannel(string channelName)
        {
            this.ChannelName = channelName;
        }

        /// <summary>
        /// This will run the initial actions needed by the custom output adapter
        /// before it is started.
        /// </summary>
        public virtual void DoStartActivities()
        {
        }

        /// <summary>
        /// This will run the clean-up actions needed by the custom output adapter
        /// before it is stopped.
        /// </summary>
        public virtual void DoStopActivities()
        {
        }

        /// <summary>
        /// This is the extention point where all input adapter implemenations will extract a message from 
        /// their defined storage location by the appropriate technology.
        /// </summary>
        /// <returns></returns>
        public abstract void DoSend(IEnvelope envelope);

        /// <summary>
        /// This will send the message to the storage location for processing. This will delegate to the <seealso cref="DoSend"/>
        /// method for actual implementation.
        /// </summary>
        public void Send(IEnvelope envelope)
        {
            var payload = new byte[] { };

            try
            {
                DoStartActivities();
            }
            catch (Exception exception)
            {
                if (!OnAdapterError(exception))
                    throw;
            }

            try
            {
                // invoke the send pipeline for the message:
                if (this.Pipeline != null)
                    envelope = this.Pipeline.Invoke(PipelineDirection.Send, envelope);

                // make sure the payload for delivery is in byte[] format for the adapter
                // with the exception of the in-memory queue channel adapter (pass as-is):
                if (!(this.GetType() == typeof(QueueChannelOutputAdapter)))
                {
                    try
                    {
                        var contents = envelope.Body.GetPayload<object>();

                        if (contents is string)
                            payload = ASCIIEncoding.ASCII.GetBytes(contents as string);
                        else
                        {
                            payload = envelope.Body.GetPayload<byte[]>();
                        }

                        envelope.Body.SetPayload(payload);
                    }
                    catch
                    {
                        // this may or may not be needed based on the adapter but do it anyway...
                    }
                }

                // delegate to the concrete implementation:
                this.DoSend(envelope);
                OnMessageDelivered(envelope);
            }
            catch (AdapterNonDeliveredMessageException exception)
            {
                if (!OnAdapterError(exception))
                    throw;
            }
            catch (Exception exception)
            {
                if (!OnAdapterError(exception))
                    throw;
            }

            try
            {
                DoStopActivities();
            }
            catch (Exception exception)
            {
                if (!OnAdapterError(exception))
                    throw;
            }
        }


        private void PushToAlternateLocation(IEnvelope envelope)
        {
            try
            {
                if(this.RetryStrategy == null) return;

                if(string.IsNullOrEmpty(this.RetryStrategy.FailureDeliveryUri)) return;

                var template = ObjectBuilder.Resolve<IAdapterMessagingTemplate>();

                if(template == null) return;

                ObjectBuilder.Resolve<IAdapterMessagingTemplate>().DoSend(new Uri(this.RetryStrategy.FailureDeliveryUri), envelope);
            }
            catch (Exception exception)
            {
                if(!OnAdapterError(exception))
                throw;
            }
        }

        private void ClearChannelMessages()
        {
            if(this.ObjectBuilder == null) return;

            var channel = this.ObjectBuilder.Resolve<IChannelRegistry>()
                .FindChannel(this.ChannelName);

            if (channel is NullChannel)
                return;

            if (!(channel is QueueChannel))
                return;

            var queueChannel = channel as QueueChannel;

            if (queueChannel.GetMessages() == null)
                return;

            while (queueChannel.GetMessages().Count > 0)
            {
                try
                {
                    this.Send(queueChannel.Receive(1));
                }
                catch
                {
                    continue;
                }

            }
        }

        private void Retry(IEnvelope envelope)
        {
            var isMessageDelivered = false;
            Exception retryException = null;

            if (RetryStrategy == null)
                RetryStrategy = new RetryStrategy();

            for (var i = 0; i < RetryStrategy.MaxRetries; i++)
            {
                if (isMessageDelivered)
                    break;

                try
                {
                    this.DoSend(envelope);
                    isMessageDelivered = true;
                }
                catch (Exception exception)
                {
                    retryException = exception;
                    if (RetryStrategy.WaitInterval > 0)
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(RetryStrategy.WaitInterval));

                    continue;
                }
            }

            if (!isMessageDelivered)
            {
                var msg = "The message could not be delivered to the destination of " + this.Uri + ". ";

                if (retryException != null)
                    msg = msg + "Reason: " + retryException.ToString();

                throw new AdapterNonDeliveredMessageException(msg, retryException, this.Uri, envelope);
            }
        }

        private void OnMessageDelivered(IEnvelope envelope)
        {
            EventHandler<ChannelAdapterMessageDeliveredEventArgs> evt =
                this.AdapterMessgeDelivered;

            if (evt != null)
                evt(this, new ChannelAdapterMessageDeliveredEventArgs(envelope, this.ChannelName, this.Uri));
        }

        private bool OnAdapterError(Exception exception)
        {
            EventHandler<ChannelAdapterErrorEventArgs> evt = this.AdapterError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new ChannelAdapterErrorEventArgs(exception));

            return isHandlerAttached;
        }

        private void OnAdapterStarted(string message)
        {
            EventHandler<ChannelAdapterStartedEventArgs> evt = this.AdapterStarted;
            if (evt != null)
                evt(this, new ChannelAdapterStartedEventArgs(string.Format("Output adapter for channel '{0}' on uri '{1}' started.", this.ChannelName, this.Uri)));
        }

        private void OnAdapterStopped(string message)
        {
            EventHandler<ChannelAdapterStoppedEventArgs> evt = this.AdapterStopped;
            if (evt != null)
                evt(this, new ChannelAdapterStoppedEventArgs(string.Format("Output adapter for channel '{0}' on uri '{1}' stopped.", this.ChannelName, this.Uri)));
        }

        private void Adapter_ServiceStarted(object sender, BackGroundServiceEventArgs e)
        {
            OnAdapterStarted(e.Message);
        }

        private void Adapter_ServiceStopped(object sender, BackGroundServiceEventArgs e)
        {
            OnAdapterStopped(e.Message);
        }

        private void Adapter_ServiceError(object sender, BackGroundServiceErrorEventArgs e)
        {
            OnAdapterError(e.Exception);
        }

        private void PipelineStarted(object sender, SendPipelineStartedEventArgs e)
        {
            var template = ObjectBuilder.Resolve<IAdapterMessagingTemplate>();
            if (template == null) return;

            var pipeline = sender as AbstractSendPipeline;
            var msg = string.Format("Send pipeline '{0}' for channel '{1}' on uri '{2}' started.", 
                pipeline.Name,
                this.ChannelName, 
                this.Uri);

            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }

        private void PipelineComponentInvoked(object sender, SendPipelineComponentInvokedEventArgs e)
        {
            var template = ObjectBuilder.Resolve<IAdapterMessagingTemplate>();

            if (template == null) return;

            var pipeline = sender as AbstractSendPipeline;
            var msg = string.Format("Send pipeline '{0}' component '{1}' invoked for channel '{2}' on uri '{3}' with message '{4}'.",
                pipeline.Name,
                e.PipelineComponent.Name,
                this.ChannelName,
                this.Uri,
                e.Envelope.Body.GetPayload<object>().GetType().FullName);

            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }

        private void PipelineCompleted(object sender, SendPipelineCompletedEventArgs e)
        {
            var template = ObjectBuilder.Resolve<IAdapterMessagingTemplate>();
            if (template == null) return;

            var pipeline = sender as AbstractSendPipeline;
            var msg = string.Format("Send pipeline '{0}' for channel '{1}' on uri '{2}' completed.", pipeline.Name,
                                    this.ChannelName, this.Uri);
            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));
        }

        private void PipelineError(object sender, SendPipelineErrorEventArgs e)
        {
            var template = ObjectBuilder.Resolve<IAdapterMessagingTemplate>();

            if (template == null) return;

            var location = "{Not Specifed}";

            if (this.RetryStrategy != null)
                if (!string.IsNullOrEmpty(this.RetryStrategy.FailureDeliveryUri))
                    location = this.RetryStrategy.FailureDeliveryUri;

            var pipeline = sender as AbstractSendPipeline;
            var msg = string.Format("Send pipeline '{0}' for channel '{1}' on uri '{2}' encountered an error for component '{3}'. Pushing to location: '{4}'", 
                    pipeline.Name,
                    this.ChannelName, 
                    this.Uri, 
                    e.PipelineComponent.Name,
                    location);

            template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(msg));

            this.PushToAlternateLocation(e.Envelope);
        }
    }
}