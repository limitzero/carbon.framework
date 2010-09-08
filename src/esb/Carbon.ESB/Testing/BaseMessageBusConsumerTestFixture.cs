using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Channel.Template;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.ESB.Configuration;
using Carbon.ESB.Saga.Persister;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;

namespace Carbon.ESB.Testing
{
    /// <summary>
    /// Base test fixture for setting up the infrastructure to send messages to a component
    /// via the message bus. All saga state will be kept via the in-memory repository.
    /// </summary>
    public class BaseMessageBusConsumerTestFixture
    {
        private IMessageEndpointActivator _component_message_activator;

        /// <summary>
        /// (Read-Only) The channel that the message will be sent in over for processing at 
        /// the message consumer.
        /// </summary>
        public string MessageConsumerInputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The current message received by the message consumer.
        /// </summary>
        public object MessageConsumerInputMessage { get; private set; }

        /// <summary>
        /// (Read-Only) The channel that the message will be sent in over for processing at 
        /// when the message consumer has a return value for the method that is processing 
        /// the input message.
        /// </summary>
        public string MessageConsumerOutputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The current message being sent back by the component (if any) to the 
        /// client for further processing.
        /// </summary>
        public object MessageConsumerOutputMessage { get; private set; }

        /// <summary>
        /// (Read-Only). The list of published messages inacted for the current message being published.
        /// </summary>
        public IList<object> PublishedMessages { get; private set; }

        /// <summary>
        /// This will look in the application config file for the configuration of the container
        /// </summary>
        protected BaseMessageBusConsumerTestFixture()
            : this(null)
        {
        }

        /// <summary>
        /// This will look in a custom configuration file to configure the container
        /// </summary>
        /// <param name="configurationFile">Configuration file to initialize the container</param>
        protected BaseMessageBusConsumerTestFixture(string configurationFile)
        {
            // initialize the infrastructure based on the configuration file:
            if (string.IsNullOrEmpty(configurationFile))
                Container = new WindsorContainer(new XmlInterpreter());
            else
            {
                Container = new WindsorContainer(configurationFile);
            }

            Container.Kernel.AddFacility(CarbonEsbFacility.FACILITY_ID, new CarbonEsbFacility());

            Container.Resolve<IObjectBuilder>()
                .Register(typeof(IMessageEndpointRegistry).Name,
                typeof(IMessageEndpointRegistry),
                typeof(MessageEndpointRegistry),
                ActivationStyle.AsSingleton);

            this.PublishedMessages = new List<object>();

            MessageBus = Container.Resolve<IMessageBus>();
            this.MessageBus.MessagePublished += OnMessagePublished;
        }

        /// <summary>
        /// (Read-Only). The current container holding all of the object references.
        /// </summary>
        protected IWindsorContainer Container { get; private set; }

        /// <summary>
        /// (Read-Only). The current integration context coordinating the integration components.
        /// </summary>
        protected IMessageBus MessageBus { get; private set; }

        public void CreateChannels(params string[] channels)
        {
            var registry = this.MessageBus.GetComponent<IChannelRegistry>();

            foreach (var channel in channels)
                if (registry.FindChannel(channel) is NullChannel)
                    registry.RegisterChannel(channel);
        }

        public TMessage ReceiveMessageFromChannel<TMessage>(string channel, TimeSpan? timeout)
        {
            var message = default(TMessage);
            IEnvelope envelope = null;

            var template = this.MessageBus.GetComponent<IChannelMessagingTemplate>();

            if (timeout.HasValue)
                envelope = template.DoReceive(channel, timeout.Value.Seconds);
            else
            {
                envelope = template.DoReceive(channel);
            }

            if (!(envelope is NullEnvelope))
                message = envelope.Body.GetPayload<TMessage>();

            return message;
        }

        public void SendMessageToChannel<TMessage>(string channel, TMessage message)
        {
            this.MessageBus.GetComponent<IChannelMessagingTemplate>().DoSend(channel, new Envelope(message));
        }

        /// <summary>
        /// This will receive a message from a location via the infrastructure based 
        /// on the adapter uri addressing scheme.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public TMessage ReceiveMessageFromLocation<TMessage>(string uri, TimeSpan? timeout)
        {
            var message = default(TMessage);
            IEnvelope envelope = null;

            var location = new Uri(uri);
            var template = this.MessageBus.GetComponent<IAdapterMessagingTemplate>();

            if (timeout.HasValue)
                envelope = template.DoReceive(location, timeout.Value.Seconds);
            else
            {
                envelope = template.DoReceive(location);
            }

            if (!(envelope is NullEnvelope))
                message = envelope.Body.GetPayload<TMessage>();

            return message;
        }

        /// <summary>
        /// This will send a message to a location for processing via the infrastructure
        /// per the adapter uri addressing scheme.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="uri"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        public void SendMessageToLocation<TMessage>(string uri, TMessage message, TimeSpan? timeout)
        {
            var location = new Uri(uri);
            var envelope = new Envelope(message);
            var template = this.MessageBus.GetComponent<IAdapterMessagingTemplate>();

            if (timeout.HasValue)
                template.DoSend(location, envelope, timeout.Value.Seconds);
            else
            {
                template.DoSend(location, envelope);
            }
        }

        public void RegisterInputAdapter(string uri, string channel, TimeSpan scheduled)
        {
            var adapter = MessageBus.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(channel, uri);
            adapter.Interval = scheduled.Seconds;
            MessageBus.GetComponent<IAdapterRegistry>().RegisterInputChannelAdapter(adapter);
        }

        public void RegisterInputAdapter(string uri, string channel, int concurrency, int frequency)
        {
            var adapter = MessageBus.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(channel, uri);
            adapter.Concurrency = concurrency == 0 ? 1 : concurrency;
            adapter.Frequency = frequency == 0 ? 1 : frequency;
            MessageBus.GetComponent<IAdapterRegistry>().RegisterInputChannelAdapter(adapter);
        }

        public void RegisterOutputAdapter(string uri, string channel, TimeSpan scheduled)
        {
            var adapter = MessageBus.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(channel, uri);
            adapter.Interval = scheduled.Seconds;
            MessageBus.GetComponent<IAdapterRegistry>().RegisterOutputChannelAdapter(adapter);
        }

        public void RegisterOutputAdapter(string uri, string channel, int concurrency, int frequency)
        {
            var adapter = MessageBus.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(channel, uri);
            adapter.Concurrency = concurrency == 0 ? 1 : concurrency;
            adapter.Frequency = frequency == 0 ? 1 : frequency;
            MessageBus.GetComponent<IAdapterRegistry>().RegisterOutputChannelAdapter(adapter);
        }

        /// <summary>
        /// This will register a message consumer component in the infrastructure to 
        /// accept messages over a channel and not return any response from the 
        /// method that processes the message.
        /// </summary>
        /// <typeparam name="TComponent">Component to consume the message</typeparam>
        /// <param name="inputChannel">Channel that the message will be sent in.</param>
        /// <returns></returns>
        public TComponent RegisterComponent<TComponent>(string inputChannel)
           where TComponent : class
        {
            return RegisterComponent<TComponent>(inputChannel, string.Empty);
        }

        /// <summary>
        /// This will register a message consumer component in the infrastructure to 
        /// accept messages over an input channel and return the response from the 
        /// method that processes the message to the output channel.
        /// </summary>
        /// <typeparam name="TComponent">Component to consume the message</typeparam>
        /// <param name="inputChannel">Channel that the message will be sent in.</param>
        /// <param name="outputChannel">Channel where the corresponding response will be routed to.</param>
        /// <returns></returns>
        public TComponent RegisterComponent<TComponent>(string inputChannel, string outputChannel)
            where TComponent : class
        {
            if (string.IsNullOrEmpty(inputChannel))
                throw new Exception("The component can not be manually registered without an input channel.");

            MessageBus.GetComponent<IObjectBuilder>().Register(typeof(TComponent).Name, typeof(TComponent));
            var component = MessageBus.GetComponent<TComponent>();

            // add the channels for the message movement:
            MessageBus.GetComponent<IChannelRegistry>().RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                MessageBus.GetComponent<IChannelRegistry>().RegisterChannel(outputChannel);

            // create a message endpoint activator for the component and register it:
            var registry = MessageBus.GetComponent<IMessageEndpointRegistry>();

            var endpoint = (from ep in registry.GetAllItems()
                            where ep.InputChannel.Name.Trim().ToLower() == inputChannel.Trim().ToLower() &&
                                  ep.EndpointInstanceType == typeof (TComponent)
                            select ep
                           ).FirstOrDefault();

            if(endpoint == null)
                registry.CreateEndpoint(inputChannel, outputChannel, string.Empty, component);

            foreach (var activator in registry.GetAllItems())
            {
                // inspect the endpoint instance type as the actual object is resolved at runtime:
                if (activator.EndpointInstanceType != typeof(TComponent)) continue;
                _component_message_activator = activator;
                break;
            }

            // check the endpoint to see if it is invoked and the mesage is passed in for processing:
            if (_component_message_activator != null)
            {
                _component_message_activator.MessageEndpointActivatorBeginInvoke += ComponentBeginInvoke;
                _component_message_activator.MessageEndpointActivatorEndInvoke += ComponentEndInvoke;
            }
            return component;
        }

        /// <summary>
        /// This will register a message consumer component by identifier in the infrastructure to 
        /// accept messages over a channel and not return any response from the 
        /// method that processes the message.
        /// </summary>
        /// <typeparam name="TComponent">Component to consume the message</typeparam>
        /// <param name="componentId">Unique name of the component in the container</param>
        /// <param name="inputChannel">Channel that the message will be sent in.</param>
        /// <returns></returns>
        public TComponent RegisterComponentById<TComponent>(string componentId, string inputChannel)
           where TComponent : class
        {
            return RegisterComponentById<TComponent>(componentId, inputChannel, string.Empty);
        }

        /// <summary>
        /// This will register a message consumer component by identifier in the infrastructure to 
        /// accept messages over an input channel and return the response from the 
        /// method that processes the message to the output channel.
        /// </summary>
        /// <typeparam name="TComponent">Component to consume the message</typeparam>
        /// <param name="componentId">Unique name of the component in the container</param>
        /// <param name="inputChannel">Channel that the message will be sent in.</param>
        /// <param name="outputChannel">Channel where the corresponding response will be routed to.</param>
        /// <returns></returns>
        public TComponent RegisterComponentById<TComponent>(string componentId, string inputChannel, string outputChannel)
            where TComponent : class
        {
            if (string.IsNullOrEmpty(inputChannel))
                throw new Exception("The component can not be manually registered without an input channel.");

            var component = MessageBus.GetComponent<IObjectBuilder>().Resolve(componentId);

            // add the channels for the message movement:
            MessageBus.GetComponent<IChannelRegistry>().RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                MessageBus.GetComponent<IChannelRegistry>().RegisterChannel(outputChannel);

            // create a message endpoint activator for the component and register it:
            MessageBus.GetComponent<IMessageEndpointRegistry>().CreateEndpoint(inputChannel, outputChannel, string.Empty, component);

            foreach (var activator in MessageBus.GetComponent<IMessageEndpointRegistry>().GetAllItems())
            {
                // inspect the endpoint instance type as the actual object is resolved at runtime:
                if (activator.EndpointInstanceType != typeof(TComponent)) continue;
                _component_message_activator = activator;
                break;
            }

            // check the endpoint to see if it is invoked and the mesage is passed in for processing:
            if (_component_message_activator != null)
            {
                _component_message_activator.MessageEndpointActivatorBeginInvoke += ComponentBeginInvoke;
                _component_message_activator.MessageEndpointActivatorEndInvoke += ComponentEndInvoke;
            }

            return (TComponent)component;
        }


        /// <summary>
        /// This will register a message consumer component isntance in the infrastructure to 
        /// accept messages over a channel and not return any response from the 
        /// method that processes the message.
        /// </summary>
        /// <typeparam name="TComponent">Component to consume the message</typeparam>
        /// <param name="inputChannel">Channel that the message will be sent in.</param>
        /// <returns></returns>
        public TComponent RegisterComponentInstance<TComponent>(object instance, string inputChannel)
           where TComponent : class
        {
            return RegisterComponentInstance<TComponent>(instance, inputChannel, string.Empty);
        }

        /// <summary>
        /// This will register a message consumer component instance in the infrastructure to 
        /// accept messages over an input channel and return the response from the 
        /// method that processes the message to the output channel.
        /// </summary>
        /// <typeparam name="TComponent">Component to consume the message</typeparam>
        /// <param name="instance">The current instance of the component</param>
        /// <param name="inputChannel">Channel that the message will be sent in.</param>
        /// <param name="outputChannel">Channel where the corresponding response will be routed to.</param>
        /// <returns></returns>
        public TComponent RegisterComponentInstance<TComponent>(object instance, string inputChannel, string outputChannel)
            where TComponent : class
        {
            if (typeof(TComponent).IsAssignableFrom(instance.GetType()))
                throw new Exception("The current component instance '" + instance.GetType().FullName +
                                    "' is not assignable from the type '" +
                                    typeof(TComponent).FullName);

            if (string.IsNullOrEmpty(inputChannel))
                throw new Exception("The component can not be manually registered without an input channel.");

            MessageBus.GetComponent<IObjectBuilder>().Register(instance.GetType().Name, instance, ActivationStyle.AsInstance);
            var component = MessageBus.GetComponent<TComponent>();

            // add the channels for the message movement:
            MessageBus.GetComponent<IChannelRegistry>().RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                MessageBus.GetComponent<IChannelRegistry>().RegisterChannel(outputChannel);

            // create a message endpoint activator for the component and register it:
            MessageBus.GetComponent<IMessageEndpointRegistry>().CreateEndpoint(inputChannel, outputChannel, string.Empty, component);

            foreach (var activator in MessageBus.GetComponent<IMessageEndpointRegistry>().GetAllItems())
            {
                // inspect the endpoint instance type as the actual object is resolved at runtime:
                if (activator.EndpointInstanceType != typeof(TComponent)) continue;
                _component_message_activator = activator;
                break;
            }

            // check the endpoint to see if it is invoked and the mesage is passed in for processing:
            if (_component_message_activator != null)
            {
                _component_message_activator.MessageEndpointActivatorBeginInvoke += ComponentBeginInvoke;
                _component_message_activator.MessageEndpointActivatorEndInvoke += ComponentEndInvoke;
            }
            return component;
        }

        /// <summary>
        /// Registers the saga instance for persistance (in-memory only).
        /// </summary>
        /// <typeparam name="TSaga">Type of saga to persist</typeparam>
        public void RegisterSagaPersister<TSaga>() where TSaga : Saga.Saga
        {
            Container.Resolve<IObjectBuilder>().Register(typeof(ISagaPersister<TSaga>).FullName,
                            typeof(ISagaPersister<TSaga>),
                            typeof(InMemorySagaPersister<TSaga>), ActivationStyle.AsSingleton);
        }

        public ISagaPersister<TSaga> ResolveSagaPersister<TSaga>() where TSaga : Saga.Saga
        {
            var persisterType = Container.Resolve<IReflection>()
                                    .GetGenericVersionOf(typeof(ISagaPersister<>), typeof(TSaga));
            var persister = Container.Resolve(persisterType) as ISagaPersister<TSaga>;
            return persister;
        }

        private void OnMessagePublished(object sender, MessageBusMessagePublishedEventArgs e)
        {
            this.PublishedMessages.Add(e.Message);
        }

        private void ComponentBeginInvoke(object sender, MessageEndpointActivatorBeginInvokeEventArgs eventArgs)
        {
            if (!(eventArgs.Envelope is NullEnvelope))
            {
                this.MessageConsumerInputChannel = eventArgs.Envelope.Header.InputChannel;
                this.MessageConsumerInputMessage = eventArgs.Envelope.Body.GetPayload<object>();
            }
        }

        private void ComponentEndInvoke(object sender, MessageEndpointActivatorEndInvokeEventArgs eventArgs)
        {
            if (!(eventArgs.Envelope is NullEnvelope))
            {
                this.MessageConsumerOutputChannel = eventArgs.Envelope.Header.OutputChannel;
                this.MessageConsumerOutputMessage = eventArgs.Envelope.Body.GetPayload<object>();
            }
        }

        ~BaseMessageBusConsumerTestFixture()
        {
            if (_component_message_activator != null)
            {
                _component_message_activator.MessageEndpointActivatorBeginInvoke -= ComponentBeginInvoke;
                _component_message_activator.MessageEndpointActivatorEndInvoke -= ComponentEndInvoke;
            }

            if(MessageBus.IsRunning)
                MessageBus.Stop();

            // let the bus do an ordered shut down:
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

            this.Container.Dispose();
        }
    }
}
