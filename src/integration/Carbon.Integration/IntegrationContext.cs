using System;
using System.Collections.Generic;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Pipeline;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Integration.Dsl;
using Carbon.Integration.Dsl.Surface;
using Carbon.Integration.Dsl.Surface.Registry;
using Carbon.Integration.Pipeline;
using Carbon.Integration.Scheduler;
using Carbon.Core.Channel.Template;

namespace Carbon.Integration
{
    public class IntegrationContext : IIntegrationContext
    {
        private readonly IObjectBuilder m_object_builder;
        private readonly IMessageEndpointRegistry m_message_endpoint_registry;
        private readonly IAdapterRegistry m_adapter_registry;
        private readonly IChannelRegistry m_channel_registry;
        private readonly IIntegrationMessagingPipeline m_messaging_pipeline;
        private readonly IScheduler m_scheduler;
        private bool m_disposed;


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

        public IntegrationContext(
            IObjectBuilder objectBuilder,
            IMessageEndpointRegistry messageEndpointRegistry,
            IAdapterRegistry adapterRegistry,
            IChannelRegistry channelRegistry,
            IIntegrationMessagingPipeline messagingPipeline,
            IScheduler scheduler)
        {
            m_object_builder = objectBuilder;
            m_message_endpoint_registry = messageEndpointRegistry;
            m_adapter_registry = adapterRegistry;
            m_channel_registry = channelRegistry;
            m_messaging_pipeline = messagingPipeline;
            m_scheduler = scheduler;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            if (m_disposed)
                return;

            OnContextStarted();

            ConfigureAllSurfaces();

            RecycleAdapters(true);

            RecycleMessageEndpoints(true);

            RecycleScheduler(true);

            IsRunning = true;
        }

        public void Stop()
        {
            RecycleMessageEndpoints(false);

            RecycleScheduler(false);

            RecycleAdapters(false);

            OnContextStopped();

            IsRunning = false;
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

        public TComponent GetComponent<TComponent>()
        {
            var component = default(TComponent);

            try
            {
                component = m_object_builder.Resolve<TComponent>();
            }
            catch
            {
                 throw new Exception(string.Format("There was not an implementation found in the container for requested type '{0}'.",
                   typeof(TComponent).Name));
            }

            return component;
        }

        /// <summary>
        /// This will allow for construction of a dependant component for a messaging endpoint conversation.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component to create from the existing set of  messages.</typeparam>
        public TComponent CreateComponent<TComponent>()
        {
            var component = default(TComponent);

            try
            {
                component = m_object_builder.CreateComponent<TComponent>();
            }
            catch(Exception exception)
            {
                throw new Exception(string.Format("The following component '{0}' could not be created by the context. Reason: '{1}'.",
                   typeof(TComponent).Name, 
                   exception.ToString()));
            }

            return component;
        }

        public void LoadSurface(string id)
        {
            var instance = m_object_builder.Resolve(id);

            if (!typeof(AbstractIntegrationComponentSurface).IsAssignableFrom(instance.GetType()))
                throw new ArgumentException(string.Format("The component '{0}' does not implement '{1}' as a base class for registration as an integration application design surface.",
                    instance.GetType().FullName,
                    typeof(AbstractIntegrationComponentSurface).FullName));

            var registry = m_object_builder.Resolve<ISurfaceRegistry>();

            if (!registry.Surfaces.Contains(instance as AbstractIntegrationComponentSurface))
                registry.Register(instance as AbstractIntegrationComponentSurface);

            var template = m_object_builder.Resolve<IAdapterMessagingTemplate>();
            if (template != null)
                template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                    new Envelope(string.Format("Surface '{0}' loaded.",
                        ((AbstractIntegrationComponentSurface)instance).Name)));

        }

        public void LoadSurface<TSurface>()
            where TSurface : AbstractIntegrationComponentSurface
        {
            if (!typeof(AbstractIntegrationComponentSurface).IsAssignableFrom(typeof(TSurface)))
                throw new ArgumentException(string.Format("The component '{0}' does not implement '{1}' as a base class for registration as an integration application design surface.",
                    typeof(TSurface).FullName,
                    typeof(AbstractIntegrationComponentSurface).FullName));

            this.LoadSurface(typeof(TSurface));
        }

        public void LoadSurface(Type surface, IDictionary<string, object> parameters)
        {

            if (!typeof(AbstractIntegrationComponentSurface).IsAssignableFrom(surface))
                throw new ArgumentException(string.Format("The component '{0}' does not implement '{1}' as a base class for registration as an integration application design surface.",
                    surface.FullName,
                    typeof(AbstractIntegrationComponentSurface).FullName));

            var reflection = m_object_builder.Resolve<IReflection>();
            var instance = reflection.BuildInstance(surface);

            foreach (var parameter in parameters)
            {
                try
                {
                    reflection.SetPropertyValue(instance, parameter.Key, parameter.Value);
                }
                catch
                {
                    continue;
                }
            }

            m_object_builder.Register(instance.GetType().Name, instance, ActivationStyle.AsSingleton);

            var registry = m_object_builder.Resolve<ISurfaceRegistry>();

            if (!registry.Surfaces.Contains(instance as AbstractIntegrationComponentSurface))
                registry.Register(instance as AbstractIntegrationComponentSurface);

            var template = m_object_builder.Resolve<IAdapterMessagingTemplate>();
            if (template != null)
                template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                    new Envelope(string.Format("Surface '{0}' loaded.",
                        ((AbstractIntegrationComponentSurface)instance).Name)));
        }

        public void LoadSurface(Type surface)
        {
            try
            {
                if (!typeof(AbstractIntegrationComponentSurface).IsAssignableFrom(surface))
                    throw new ArgumentException(string.Format("The component '{0}' does not implement '{1}' as a base class for registration as an integration application design surface.",
                        surface.FullName,
                        typeof(AbstractIntegrationComponentSurface).FullName));

                m_object_builder.Register(surface.Name, surface, ActivationStyle.AsSingleton);
                var instance = m_object_builder.Resolve(surface);

                var registry = m_object_builder.Resolve<ISurfaceRegistry>();

                if (!registry.Surfaces.Contains(instance as AbstractIntegrationComponentSurface))
                    registry.Register(instance as AbstractIntegrationComponentSurface);

                var template = m_object_builder.Resolve<IAdapterMessagingTemplate>();
                if (template != null)
                    template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                        new Envelope(string.Format("Surface '{0}' loaded.",
                            ((AbstractIntegrationComponentSurface)instance).Name)));

            }
            catch (Exception exception)
            {
                throw;
            }

        }

        public void LoadAllSurfaces()
        {
            // pick-up the surfaces from the executable directory:
            var scanner = m_object_builder.Resolve<IIntegrationSurfaceScanner>();
            scanner.Scan();

            // pick-up the surfaces from the configuration and display all of them:
            var registry = m_object_builder.Resolve<ISurfaceRegistry>();
            if (registry.Surfaces != null)
                foreach (var surface in registry.Surfaces)
                {
                    if (!surface.IsAvailable) continue;

                    var template = m_object_builder.Resolve<IAdapterMessagingTemplate>();
                    if (template != null)
                        template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                            new Envelope(string.Format("Surface '{0}' loaded.",
                                surface.Name)));
                }
        }

        public void Send(string channel, params IEnvelope[] messages)
        {
            try
            {
                if (m_disposed) throw new Exception("Context is disposed");

                if (!this.IsRunning)
                    OnContextError(
                        new Exception(
                            "Warning: Sending message to location '" +  channel + "' without the context being started...some messages " +
                            " may not reached their intended destination and the proper event messages may not be recorded."));

                var template = GetComponent<IChannelMessagingTemplate>();

                foreach(var message in messages)
                    template.DoSend(channel, message);
            }
            catch (Exception exception)
            {
                if (!OnContextError(exception))
                    throw;
            }
        }

        public void Send(Uri location, params IEnvelope[] messages)
        {
            try
            {
                if (m_disposed) throw new Exception("Context is disposed");

                var template = GetComponent<IAdapterMessagingTemplate>();

                foreach (var message in messages)
                    template.DoSend(location, message);
            }
            catch (Exception exception)
            {
                if (!OnContextError(exception))
                    throw;
            }
        }

        private void ConfigureAllSurfaces()
        {
            foreach (var surface in m_object_builder.Resolve<ISurfaceRegistry>().Surfaces)
            {
                if(surface.IsAvailable)
                    surface.Configure();
            }
        }

        private void RecycleScheduler(bool bindAndStart)
        {
            try
            {
                if (bindAndStart)
                {
                    m_scheduler.SchedulerItemCompleted += SchedulerItemCompleted;
                    m_scheduler.SchedulerItemError += SchedulerItemError;
                    m_scheduler.Start();
                }
                else
                {
                    m_scheduler.Stop();
                    m_scheduler.SchedulerItemCompleted -= SchedulerItemCompleted;
                    m_scheduler.SchedulerItemError -= SchedulerItemError;
                }
            }
            catch (Exception exception)
            {
                if (!OnContextError(exception))
                    throw;
            }
        }

        private void RecycleMessageEndpoints(bool bindAndStart)
        {
            try
            {
                if (bindAndStart)
                {
                    m_message_endpoint_registry.MessageEndpointActivatorBeginInvoke
                        += OnMessageActivatorBeginInvoke;

                    m_message_endpoint_registry.MessageEndpointActivatorEndInvoke
                        += OnMessageActivatorEndnvoke;

                    m_message_endpoint_registry.MessageEndpointActivatorError
                        += OnMessageActivatorError;
                }
                else
                {
                    m_message_endpoint_registry.MessageEndpointActivatorBeginInvoke
                        -= OnMessageActivatorBeginInvoke;

                    m_message_endpoint_registry.MessageEndpointActivatorEndInvoke
                        -= OnMessageActivatorEndnvoke;

                    m_message_endpoint_registry.MessageEndpointActivatorError
                        -= OnMessageActivatorError;
                }
            }
            catch (Exception exception)
            {
                if (!OnContextError(exception))
                    throw;
            }
        }

        private void RecycleAdapters(bool bindAndStart)
        {

            try
            {
                if (bindAndStart)
                {
                    if (m_adapter_registry.InputAdapters.Count > 0)
                        foreach (var adapter in m_adapter_registry.InputAdapters)
                            //adapter.AdapterMessageReceived += IntegrationContextAdapterMessageReceived;
                            adapter.RegisterActionOnMessageReceived(this.IntegrationContextAdapterMessageReceivedCallback);

                    if (m_adapter_registry.OutputAdapters.Count > 0)
                        foreach (var adapter in m_adapter_registry.OutputAdapters)
                            adapter.AdapterMessgeDelivered += IntegrationContextAdapterMessageDelivered;

                    m_adapter_registry.Start();
                }
                else
                {
                    if (m_adapter_registry.InputAdapters != null)
                        if (m_adapter_registry.InputAdapters.Count > 0)
                            foreach (var adapter in m_adapter_registry.InputAdapters)
                                adapter.AdapterMessageReceived -= IntegrationContextAdapterMessageReceived;

                    if (m_adapter_registry.OutputAdapters != null)
                        if (m_adapter_registry.OutputAdapters.Count > 0)
                            foreach (var adapter in m_adapter_registry.OutputAdapters)
                                adapter.AdapterMessgeDelivered -= IntegrationContextAdapterMessageDelivered;

                    m_adapter_registry.Stop();
                }
            }
            catch (Exception exception)
            {
                if (!OnContextError(exception))
                    throw;
            }

        }

        private bool OnContextError(Exception exception)
        {
            return OnContextError(null, exception);
        }

        private bool OnContextError(IEnvelope envelope, Exception exception)
        {
            EventHandler<ApplicationContextErrorEventArgs> eventHandler = this.ApplicationContextError;
            var isHandlerAttached = (eventHandler != null);

            try
            {
                var message = string.Empty;

                if (envelope != null || envelope is NullEnvelope)
                    message = string.Format(
                        "An exception has ocurred while attempting to process the message '{0}'. Exception: {1}",
                        envelope.Body.GetPayload<object>().GetType().FullName,
                        exception.ToString());
                else
                {
                    message = string.Format(
                    "An exception has ocurred within the integration context. Exception: {0}",
                    exception.ToString());
                }

                var ex = new Exception(message, exception);

                if (isHandlerAttached)
                    eventHandler(this, new ApplicationContextErrorEventArgs(message, ex));
                else
                {
                    var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                    messageTemplate.DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI),
                                           new Envelope(message));
                }

            }
            catch (Exception ex)
            {
                throw;
            }

            return isHandlerAttached;
        }

        private void OnContextStarted()
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI),
                                       new Envelope("Context started."));
            }
            catch (Exception exception)
            {
                if (!OnContextError(null, exception))
                    throw;
            }
        }

        private void OnContextStopped()
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI),
                                       new Envelope("Context stopped."));
            }
            catch (Exception exception)
            {
                if (!OnContextError(null, exception))
                    throw;
            }
        }

        private void IntegrationContextAdapterError(object sender, ChannelAdapterErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void IntegrationContextAdapterMessageDelivered(object sender, ChannelAdapterMessageDeliveredEventArgs e)
        {
            try
            {
                var message = string.Format("Message '{0}' delivered to target '{1}' via channel '{2}'.",
                                            e.Envelope.Body.GetPayload<object>().GetType().FullName,
                                            e.Uri,
                                            e.Channel);

                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(message));
            }
            catch (Exception exception)
            {
                if (!OnContextError(null, exception))
                    throw;
            }
        }

        private void IntegrationContextAdapterMessageReceivedCallback(ChannelAdapterMessageReceivedEventArgs eventArgs)
        {
            try
            {
                this.IntegrationContextAdapterMessageReceived(this, eventArgs);
            }
            catch (Exception exception)
            {
                if (!OnContextError(eventArgs.Envelope, exception))
                    throw;
            }

        }

        private void IntegrationContextAdapterMessageReceived(object sender, ChannelAdapterMessageReceivedEventArgs e)
        {
            try
            {
                var message = string.Format("Message '{0}' received from target '{1}' via channel '{2}'.",
                                            e.Envelope.Body.GetPayload<object>().GetType().FullName,
                                            e.Uri,
                                            e.Channel);

                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(message));

            }
            catch (Exception exception)
            {
                if (!OnContextError(e.Envelope, exception))
                    throw;
            }

            try
            {
                this.DispatchToEndpoint(e.Channel, e.Envelope);
            }
            catch (Exception exception)
            {
                if (!OnContextError(e.Envelope, exception))
                    throw;
            }
        }

        private void SchedulerItemError(object sender, ScheduledItemErrorEventArgs e)
        {
            this.OnContextError(e.Exception);
        }

        private void SchedulerItemCompleted(object sender, SchedulerItemCompletedEventArgs e)
        {
            // pass the message to the next component for processing:
            this.DispatchToEndpoint(e.Message.Header.OutputChannel, e.Message);
        }

        private void DispatchToEndpoint(string channel, IEnvelope envelope)
        {
            try
            {
                envelope.Header.InputChannel = channel;
                this.m_messaging_pipeline.Invoke(PipelineDirection.Receive, envelope);
            }
            catch (Exception exception)
            {
                // route the message to the appropriate error port, if none is 
                // defined, then send to the error log:
                throw;
            }

        }

        private void OnMessageActivatorBeginInvoke(object sender, MessageEndpointActivatorBeginInvokeEventArgs e)
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(e.Message));
            }
            catch (Exception exception)
            {
                if (!OnContextError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }

        }

        private void OnMessageActivatorEndnvoke(object sender, MessageEndpointActivatorEndInvokeEventArgs e)
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                       new Envelope(e.Message));
            }
            catch (Exception exception)
            {
                if (!OnContextError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }
        }

        private void OnMessageActivatorError(object sender, MessageEndpointActivatorErrorEventArgs e)
        {
            try
            {
                var messageTemplate = this.GetComponent<IAdapterMessagingTemplate>();
                messageTemplate.DoSend(new Uri(Constants.LogUris.ERROR_LOG_URI),
                                       new Envelope(e.Exception.ToString()));
            }
            catch (Exception exception)
            {
                if (!OnContextError(e.Envelope, new Exception(e.Message, exception)))
                    throw;
            }
        }
    }
}