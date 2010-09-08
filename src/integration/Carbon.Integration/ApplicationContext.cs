using System;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Builder;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Registries.For.ServiceEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Stereotypes.For.Components.Service.Impl;

namespace Carbon.Integration
{
    /// <summary>
    /// Core context used to hold resources for application integration message passing.
    /// </summary>
    public class ApplicationContext : IApplicationContext
    {
        private readonly IObjectBuilder m_container;
        private readonly IMessageEndpointRegistry m_message_endpoint_registry;
        private readonly IServiceEndpointRegistry m_service_endpoint_registry;
        private readonly IAdapterRegistry m_adapter_registry;

        /// <summary>
        /// Event that is triggered when a message is delivered to a messaging endpoint.
        /// </summary>
        public event EventHandler<ApplicationContextMessageDeliveredEventArgs> ApplicationContextMessageDelivered;

        /// <summary>
        /// Event that is triggered when a message is received from a messaging endpoint.
        /// </summary>
        public event EventHandler<ApplicationContextMessageReceivedEventArgs> ApplicationContextMessageReceived;

        /// <summary>
        /// Event that is triggered when a message endpoint encounters an error.
        /// </summary>
        public event EventHandler<ApplicationContextErrorEventArgs> ApplicationContextError;

        public bool IsRunning { get; private set; }

        public ApplicationContext(IObjectBuilder container,
                                  IMessageEndpointRegistry messageEndpointRegistry, 
                                  IServiceEndpointRegistry serviceEndpointRegistry, 
                                  IAdapterRegistry adapterRegistry)
        {
            m_container = container;
            m_message_endpoint_registry = messageEndpointRegistry;
            m_service_endpoint_registry = serviceEndpointRegistry;
            m_adapter_registry = adapterRegistry;
        }

        public TComponent GetComponent<TComponent>()
        {
            return m_container.Resolve<TComponent>();
        }

        public void Dispose()
        {
            this.Stop();
        }

        public void Start()
        {
            if(IsRunning)
                return;

            m_message_endpoint_registry.MessageEndpointActivatorBeginInvoke
                += OnMessageActivatorBeginInvoke;

            m_message_endpoint_registry.MessageEndpointActivatorEndInvoke
                += OnMessageActivatorEndnvoke;

            m_message_endpoint_registry.MessageEndpointActivatorError
                += OnMessageActivatorError;

            m_adapter_registry.Start();

            IsRunning = true;

            OnApplicationStarted();
        }

        public void Stop()
        {
            m_message_endpoint_registry.MessageEndpointActivatorBeginInvoke
                -= OnMessageActivatorBeginInvoke;

            m_message_endpoint_registry.MessageEndpointActivatorEndInvoke
                -= OnMessageActivatorEndnvoke;

            m_message_endpoint_registry.MessageEndpointActivatorError
                -= OnMessageActivatorError;

            m_adapter_registry.Stop();

            IsRunning = false;
            OnApplicationContextStopped();
        }

        public   void RegisterServiceEndpointActivator(IServiceActivator serviceActivator)
        {
            m_service_endpoint_registry.Register(serviceActivator);
        }

        public void RegisterMessageEndpointActivator(IMessageEndpointActivator messageEndpointActivator)
        {
            m_message_endpoint_registry.Register(messageEndpointActivator);
        }

        public void RegisterInputChannelAdapter(AbstractInputChannelAdapter adapter)
        {
            m_adapter_registry.RegisterInputChannelAdapter(adapter);
        }

        public void RegisterOutputChannelAdapter(AbstractOutputChannelAdapter adapter)
        {
            m_adapter_registry.RegisterOutputChannelAdapter(adapter);
        }

        private void OnApplicationStarted()
        {
            try
            {
                var messageTemplate = m_container.Resolve<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope("Application context started"));
            }
            catch (Exception exception)
            {
                if (!OnApplicationContextError(null, exception))
                    throw;
            }
        }

        private void OnApplicationContextStopped()
        {
            try
            {
                var messageTemplate = m_container.Resolve<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope("Application context stopped"));
            }
            catch (Exception exception)
            {
                if (!OnApplicationContextError(null, exception))
                    throw;
            }
        }

        private void OnApplicationContextMessageReceived(object message)
        {
            EventHandler<ApplicationContextMessageReceivedEventArgs> eventHandler =
                this.ApplicationContextMessageReceived;
            if (eventHandler != null)
                eventHandler(this, new ApplicationContextMessageReceivedEventArgs(message));
        }

        private void OnApplicationContextMessageDelivered(string location, object message)
        {
            EventHandler<ApplicationContextMessageDeliveredEventArgs> eventHandler =
                this.ApplicationContextMessageDelivered;
            if (eventHandler != null)
                eventHandler(this, new ApplicationContextMessageDeliveredEventArgs(location, message));
        }

        private bool OnApplicationContextError(object message, Exception exception)
        {
            EventHandler<ApplicationContextErrorEventArgs> eventHandler = this.ApplicationContextError;
            var isHandlerAttached = (eventHandler != null);

            if (isHandlerAttached)
                eventHandler(this, new ApplicationContextErrorEventArgs(message, exception));

            return isHandlerAttached;
        }

        private void OnMessageActivatorBeginInvoke(object sender,
                                                   MessageEndpointActivatorBeginInvokeEventArgs e)
        {
            try
            {
                var messageTemplate = m_container.Resolve<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(e.Message));
            }
            catch (Exception exception)
            {
                if (!OnApplicationContextError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }

        }

        private void OnMessageActivatorEndnvoke(object sender,
                                                MessageEndpointActivatorEndInvokeEventArgs e)
        {
            try
            {
                var messageTemplate = m_container.Resolve<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(e.Message));
            }
            catch (Exception exception)
            {
                if (!OnApplicationContextError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }
        }

        private void OnMessageActivatorError(object sender,
                                             MessageEndpointActivatorErrorEventArgs e)
        {
            try
            {
                var messageTemplate = m_container.Resolve<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI),
                                       new Envelope(e.Exception.ToString()));
            }
            catch (Exception exception)
            {
                if (!OnApplicationContextError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }
        }
    }
}