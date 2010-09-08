using System;
using System.Transactions;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Internals.Serialization;
using Carbon.Core.Pipeline;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.ESB.Configuration;
using Carbon.ESB.Messages;
using Carbon.ESB.Pipeline;
using Carbon.ESB.Registries.Endpoints;
using Carbon.ESB.Saga;
using Carbon.ESB.Services.Registry;
using Carbon.Core.Adapter.Impl.Null;

namespace Carbon.ESB
{
    public class MessageBus : IMessageBus
    {
        private const int MESSAGE_BATCH_SIZE = 256;
        private readonly IObjectBuilder m_builder;
        private bool m_disposed;

        public event EventHandler<MessageBusMessageReceivedEventArgs> MessageBusMessageReceived;
        public event EventHandler<MessageBusMessageDeliveredEventArgs> MessageBusMessageDelivered;
        public event EventHandler<MessageBusErrorEventArgs> MessageBusError;
        public event EventHandler<MessageBusMessagePublishedEventArgs> MessagePublished;

        /// <summary>
        /// (Read-Only). Flag to indicate whether the component is started or not.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// (Read-Write). The channel adapter that will represent the bus listening for messages and writing 
        /// them to a local channel for processing out to message endpoints.
        /// </summary>
        public AbstractInputChannelAdapter Endpoint { get; set; }

        /// <summary>
        /// (Read-Write). The logical channel where the bus is located.
        /// </summary>
        public string LocalChannel { get; set; }

        /// <summary>
        /// (Read-Write). The address where the bus is located.
        /// </summary>
        public string LocalAddress { get; set; }

        /// <summary>
        /// (Read-Write). The address where all remote subscriptions that can not be resolved locally are sent 
        /// for resolution for publication.
        /// </summary>
        public string SubscriptionAddress { get; set; }

        /// <summary>
        /// (Read-Write). The value set from the configuration to determine whether or not the end points 
        /// will be configured from their code definition (i.e. attributes) or the configuration file for messaging.
        /// </summary>
        public bool IsAnnotationDriven { get; set; }

        public MessageBus(IObjectBuilder builder)
        {
            m_builder = builder;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            if (m_disposed)
                return;

            try
            {
                var template = m_builder.Resolve<IAdapterMessagingTemplate>();

                // start the bus adapter:
                if (Endpoint != null)
                {
                    Endpoint.AdapterMessageReceived += BusEndpointMessageReceived;
                    Endpoint.AdapterError += AdapterError;
                    Endpoint.AdapterStarted += AdapterStarted;
                    Endpoint.AdapterStopped += AdapterStopped;
                    Endpoint.Start();
                }

                // start all of the endpoint adapters:
                var adapterRegistry = this.GetComponent<IAdapterRegistry>();

                if (adapterRegistry.InputAdapters != null)
                    foreach (var inputAdapter in adapterRegistry.InputAdapters)
                    {
                        if (inputAdapter is NullInputChannelAdapter)
                        {
                            var msg =
                                string.Format(
                                    "Warning: The following channel '{0}' is defined for accepting messages but does not have a transport adapter assigned for transmitting/receiving messages. This channel will not be able to transmit or receive messages." +
                                    "Please ensure that the appropriate adapter for addressing scheme '{1}' is included in the executable directory for the message bus process to pick up the definition for registration.",
                                    inputAdapter.ChannelName, inputAdapter.Uri);
                            template.DoSend(new Uri(Constants.LogUris.WARN_LOG_URI), new Envelope(msg));
                        }
                        else
                        {
                            inputAdapter.AdapterMessageReceived += EndpointMessageReceived;
                            inputAdapter.AdapterError += AdapterError;
                            inputAdapter.AdapterStarted += AdapterStarted;
                            inputAdapter.AdapterStopped += AdapterStopped;
                            inputAdapter.Start();
                        }
                    }

                if (adapterRegistry.OutputAdapters != null)
                    foreach (var outputAdapter in adapterRegistry.OutputAdapters)
                    {
                        outputAdapter.AdapterError += AdapterError;
                        outputAdapter.AdapterStarted += AdapterStarted;
                        outputAdapter.AdapterStopped += AdapterStopped;
                        outputAdapter.Start();
                    }

                var messageEndpointRegistry = this.GetComponent<IServiceBusEndpointRegistry>();
                var backGroundServiceRegistry = this.GetComponent<IBackgroundServiceRegistry>();

                if (messageEndpointRegistry != null)
                {
                    messageEndpointRegistry.MessageEndpointActivatorBeginInvoke
                        += OnMessageActivatorBeginInvoke;

                    messageEndpointRegistry.MessageEndpointActivatorEndInvoke
                        += OnMessageActivatorEndnvoke;

                    messageEndpointRegistry.MessageEndpointActivatorError
                        += OnMessageActivatorError;

                    messageEndpointRegistry.SetMessageBus(this);
                }

                if (backGroundServiceRegistry != null)
                {
                    backGroundServiceRegistry.SetMessageBus(this);
                    backGroundServiceRegistry.Start();
                }

                IsRunning = true;
                OnServiceBusStarted();
            }
            catch (Exception exception)
            {
                this.Stop();

                if (!OnServiceBusError(null, exception))
                    throw;
            }

        }

        public void Stop()
        {
            try
            {

                // stop the bus adapter:
                if (Endpoint != null)
                {
                    Endpoint.Stop();
                    Endpoint.AdapterMessageReceived -= BusEndpointMessageReceived;
                    Endpoint.AdapterError -= AdapterError;
                    Endpoint.AdapterStarted -= AdapterStarted;
                    Endpoint.AdapterStopped -= AdapterStopped;
                }

                // stop all of the endpoint adapters:
                var adapterRegistry = this.GetComponent<IAdapterRegistry>();

                if (adapterRegistry.InputAdapters != null)
                    foreach (var inputAdapter in adapterRegistry.InputAdapters)
                    {
                        inputAdapter.Stop();
                        inputAdapter.AdapterMessageReceived -= EndpointMessageReceived;
                        inputAdapter.AdapterError -= AdapterError;
                        inputAdapter.AdapterStarted -= AdapterStarted;
                        inputAdapter.AdapterStopped -= AdapterStopped;
                    }

                if (adapterRegistry.OutputAdapters != null)
                    foreach (var outputAdapter in this.GetComponent<IAdapterRegistry>().OutputAdapters)
                    {
                        outputAdapter.Stop();
                        outputAdapter.AdapterError -= AdapterError;
                        outputAdapter.AdapterStarted -= AdapterStarted;
                        outputAdapter.AdapterStopped -= AdapterStopped;
                    }

                var messageEndpointRegistry = this.GetComponent<IServiceBusEndpointRegistry>();
                var backGroundServiceRegistry = this.GetComponent<IBackgroundServiceRegistry>();

                if (backGroundServiceRegistry != null)
                    if (backGroundServiceRegistry.IsRunning)
                        backGroundServiceRegistry.Stop();

                messageEndpointRegistry.MessageEndpointActivatorBeginInvoke
                    -= OnMessageActivatorBeginInvoke;

                messageEndpointRegistry.MessageEndpointActivatorEndInvoke
                    -= OnMessageActivatorEndnvoke;

                messageEndpointRegistry.MessageEndpointActivatorError
                    -= OnMessageActivatorError;
            }
            catch (Exception exception)
            {
                if (!OnServiceBusError(null, exception))
                    throw;
            }

            IsRunning = false;
            OnServiceBusStopped();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    this.Stop();
                }

                m_disposed = true;
            }
        }

        public void Configure<TEndpointConfiguration>()
            where TEndpointConfiguration : AbstractBootStrapper, new()
        {
            var configuration = new TEndpointConfiguration() { Builder = this.m_builder };
            configuration.Configure();
        }

        /// <summary>
        /// This will allow for retrieval of a dependant component for a
        /// messaging endpoint conversation.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component to retrieve from the component container.</typeparam>
        /// <returns></returns>
        public TComponent GetComponent<TComponent>()
        {
            return m_builder.Resolve<TComponent>();
        }

        /// <summary>
        /// This will allow for construction of a dependant component for a
        /// messaging endpoint conversation.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component to create from the existing set of conversation messages.</typeparam>
        public TComponent CreateComponent<TComponent>() where TComponent : ISagaMessage
        {
            return m_builder.CreateComponent<TComponent>();
        }

        /// <summary>
        /// This will take a message that is on the persistant storage location for the end point
        /// and dispatch it to the component for processing.
        /// </summary>
        /// <param name="message"></param>
        public virtual void Publish(IEnvelope message)
        {
            if (message is NullEnvelope) return;
            var messageToPublish = message.Body.GetPayload<object>();

            using (var txn = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    this.Publish(messageToPublish);
                }
                catch (Exception exception)
                {
                    if (!OnServiceBusError(messageToPublish, exception))
                        throw;
                }
            }

        }

        /// <summary>
        /// This will send a message out to the set of messaging endpoints for an existing conversation 
        /// that can process the message.
        /// </summary>
        /// <param name="messages">Message(s) to send</param>
        public virtual void Publish(params object[] messages)
        {
            if (messages.Length > MESSAGE_BATCH_SIZE)
                throw new Exception(string.Format("The message batch can not exceed {0} items.",
                    MESSAGE_BATCH_SIZE.ToString()));

            using (var txn = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                object currentMsg = null;

                try
                {
                    foreach (var msg in messages)
                    {
                        currentMsg = msg;
                        this.PublishInternal(null, msg);
                    }

                    txn.Complete();
                }
                catch (Exception exception)
                {
                    if (!OnServiceBusError(currentMsg, exception))
                        throw;
                }
            }
        }

        /// <summary>
        /// This will send a message out to the set of messaging endpoints for an existing conversation 
        /// that can process the message.
        /// </summary>
        /// <typeparam name="TMessage">Type of the message to send</typeparam>
        /// <param name="saga">The existing conversation that will publish out a message.</param>
        /// <param name="messages">Message(s) to send</param>
        public virtual void Publish<TMessage>(ISaga saga, params TMessage[] messages)
            where TMessage : class, ISagaMessage
        {
            if (messages.Length > MESSAGE_BATCH_SIZE)
                throw new Exception(string.Format("The message batch can not exceed {0} items.",
                    MESSAGE_BATCH_SIZE.ToString()));

            using (var txn = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TMessage currentMessage = null;

                try
                {
                    foreach (var msg in messages)
                    {
                        msg.SagaId = saga.SagaId;
                        currentMessage = msg;
                        this.PublishInternal(saga, msg);
                    }

                    txn.Complete();
                }
                catch (Exception exception)
                {
                    if (!OnServiceBusError(currentMessage, exception))
                        throw;
                }

            }

        }

        /// <summary>
        /// This will send a message indicating that the initial message
        /// that is queued for delayed delivered should be cancelled 
        /// for the existing conversation.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to look for for cancellation.</typeparam>
        public virtual void CancelPublication<TMessage>(ISaga saga)
            where TMessage : ISagaMessage, new()
        {
            var message = new TMessage();
            message.SagaId = saga.SagaId;
            var cm = new CancelTimeoutMessage(message);

            using (var txn = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    this.PublishInternal(saga, cm);
                    txn.Complete();
                }
                catch (Exception exception)
                {
                    if (!OnServiceBusError(cm, exception))
                        throw;
                }
            }

        }

        /// <summary>
        /// This will delay the publication of a message to the set of messaging endponits
        /// that can process the message.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to send</typeparam>
        /// <param name="saga"></param>
        /// <param name="waitInterval"><see cref="TimeSpan"/>Timespan to wait before delivery</param>
        /// <param name="message">Message to deliver</param>
        public void DelayPublication<TMessage>(ISaga saga, TimeSpan waitInterval, TMessage message)
            where TMessage : class, ISagaMessage
        {
            message.SagaId = saga.SagaId;
            var tm = new TimeoutMessage(waitInterval, message);

            using (var txn = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                try
                {
                    this.PublishInternal(saga, tm);
                    txn.Complete();
                }
                catch (Exception exception)
                {
                    if (!OnServiceBusError(message, exception))
                        throw;
                }
            }

        }

        private bool PublishInternal(ISaga saga, object message)
        {
            OnServiceBusMessageReceived(message);

            try
            {
                IEnvelope envelope = new Envelope(message);

                if (typeof(ISagaMessage).IsAssignableFrom(message.GetType()))
                    envelope.Header.CorrelationId = ((ISagaMessage)message).SagaId.ToString();

                var pipeline = this.GetComponent<IMessageBusMessagingPipeline>();

                pipeline.Invoke(PipelineDirection.Send, envelope);

                OnServiceBusMessageDelivered(message);
            }
            catch (Exception exception)
            {
                //if(!OnServiceBusError(message, exception))
                throw;
            }

            return true;
        }

        private void AdapterError(object sender, ChannelAdapterErrorEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI), new Envelope(e.Message + " " + e.Exception.ToString()));
        }

        private void AdapterStopped(object sender, ChannelAdapterStoppedEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI), new Envelope(e.Message));
        }

        private void AdapterStarted(object sender, ChannelAdapterStartedEventArgs e)
        {
            var template = m_builder.Resolve<IAdapterMessagingTemplate>();
            template.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI), new Envelope(e.Message));
        }

        private void BusEndpointMessageReceived(object sender, ChannelAdapterMessageReceivedEventArgs e)
        {
            PrepareMessageForDispatch(e.Envelope);
        }

        private void EndpointMessageReceived(object sender, ChannelAdapterMessageReceivedEventArgs e)
        {
            PrepareMessageForDispatch(e.Envelope);
        }

        private IEnvelope PrepareMessageForSend(IEnvelope envelope, object message)
        {
            var serializer = m_builder.Resolve<ISerializationProvider>();
            var payload = serializer.SerializeToBytes(message);
            envelope.Body.SetPayload(payload);
            return envelope;
        }

        private void PrepareMessageForDispatch(IEnvelope envelope)
        {
            if (envelope is NullEnvelope) return;

            var pipeline = this.GetComponent<IMessageBusMessagingPipeline>();
            var receivedEnvelope = pipeline.Invoke(PipelineDirection.Receive, envelope);
            //this.PublishInternal(null, receivedEnvelope.Body.GetPayload<object>());
        }

        private void OnServiceBusStarted()
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI),
                                       new Envelope("Message bus started"));
            }
            catch (Exception exception)
            {
                if (!OnServiceBusError(null, exception))
                    throw;
            }
        }

        private void OnServiceBusStopped()
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI),
                                       new Envelope("Message bus stopped."));
            }
            catch (Exception exception)
            {
                if (!OnServiceBusError(null, exception))
                    throw;
            }
        }

        private void OnServiceBusMessageReceived(object message)
        {
            EventHandler<MessageBusMessageReceivedEventArgs> eventHandler =
                this.MessageBusMessageReceived;

            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(string.Format("Message received by the  bus: '{0}'", message.GetType().FullName)));
            }
            catch (Exception exception)
            {
                if (!OnServiceBusError(null, exception))
                    throw;
            }

            if (eventHandler != null)
                eventHandler(this, new MessageBusMessageReceivedEventArgs(message));
        }

        private void OnServiceBusMessageDelivered(object message)
        {
            EventHandler<MessageBusMessageDeliveredEventArgs> eventHandler =
                this.MessageBusMessageDelivered;

            OnMessagePublished(message);

            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(string.Format("Message delivered via bus: '{0}'", message.GetType().FullName)));
            }
            catch (Exception exception)
            {
                if (!OnServiceBusError(null, exception))
                    throw;
            }

            if (eventHandler != null)
                eventHandler(this, new MessageBusMessageDeliveredEventArgs(string.Empty, message));
        }

        private void OnMessagePublished(object message)
        {
            var eventHandler = this.MessagePublished;
            if (eventHandler != null)
                eventHandler(this, new MessageBusMessagePublishedEventArgs(message));
        }

        private bool OnServiceBusError(object message, Exception exception)
        {
            EventHandler<MessageBusErrorEventArgs> eventHandler = this.MessageBusError;
            var isHandlerAttached = (eventHandler != null);

            if (isHandlerAttached)
                eventHandler(this, new MessageBusErrorEventArgs(message, exception));

            return isHandlerAttached;
        }

        private void OnMessageActivatorBeginInvoke(object sender,
                                                   MessageEndpointActivatorBeginInvokeEventArgs e)
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(e.Message));
            }
            catch (Exception exception)
            {
                if (!OnServiceBusError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }

        }

        private void OnMessageActivatorEndnvoke(object sender,
                                                MessageEndpointActivatorEndInvokeEventArgs e)
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(e.Message));
            }
            catch (Exception exception)
            {
                if (!OnServiceBusError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }
        }

        private void OnMessageActivatorError(object sender,
                                             MessageEndpointActivatorErrorEventArgs e)
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI),
                                       new Envelope(e.Exception.ToString()));
            }
            catch (Exception exception)
            {
                if (!OnServiceBusError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }
        }

    }
}