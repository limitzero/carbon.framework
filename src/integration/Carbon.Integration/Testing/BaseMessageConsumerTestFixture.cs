using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Template;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Integration.Configuration;
using Carbon.Integration.Stereotypes.Gateway.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Carbon.Core.Channel.Impl.Null;

namespace Carbon.Integration.Testing
{
    /// <summary>
    /// base test fixture for setting up the infrastructure to send messages to a component:
    /// </summary>
    public class BaseMessageConsumerTestFixture
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
        /// This will look in the application config file for the configuration of the container
        /// </summary>
        protected BaseMessageConsumerTestFixture()
            : this(null)
        {
        }

        /// <summary>
        /// This will look in a custom configuration file to configure the container
        /// </summary>
        /// <param name="configurationFile">Configuration file to initialize the container</param>
        protected BaseMessageConsumerTestFixture(string configurationFile)
        {
            // initialize the infrastructure based on the configuration file:
            if (string.IsNullOrEmpty(configurationFile))
                Container = new WindsorContainer(new XmlInterpreter());
            else
            {
                Container = new WindsorContainer(configurationFile);
            }

            Container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            Context = Container.Resolve<IIntegrationContext>();
        }

        /// <summary>
        /// (Read-Only). The current container holding all of the object references.
        /// </summary>
        protected IWindsorContainer Container { get; private set; }

        /// <summary>
        /// (Read-Only). The current integration context coordinating the integration components.
        /// </summary>
        protected IIntegrationContext Context { get; private set; }

        public void CreateChannels(params string[] channels)
        {
            var registry = this.Context.GetComponent<IChannelRegistry>();

            foreach (var channel in channels)
                if (registry.FindChannel(channel) is NullChannel)
                    registry.RegisterChannel(channel);
        }

        public TMessage ReceiveMessageFromChannel<TMessage>(string channel, TimeSpan? timeout)
        {
            var message = default(TMessage);
            IEnvelope envelope = null;

            var template = this.Context.GetComponent<IChannelMessagingTemplate>();

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
            this.Context.GetComponent<IChannelMessagingTemplate>().DoSend(channel, new Envelope(message));
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
            var template = this.Context.GetComponent<IAdapterMessagingTemplate>();

            if(timeout.HasValue)
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
            var template = this.Context.GetComponent<IAdapterMessagingTemplate>();

            if (timeout.HasValue)
                 template.DoSend(location, envelope, timeout.Value.Seconds);
            else
            {
                template.DoSend(location, envelope);
            }
        }

        public void RegisterInputAdapter(string uri, string channel, TimeSpan scheduled)
        {
            var adapter = Context.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(channel, uri);
            adapter.Interval = scheduled.Seconds;
            Context.GetComponent<IAdapterRegistry>().RegisterInputChannelAdapter(adapter);
        }

        public void RegisterInputAdapter(string uri, string channel, int concurrency, int frequency)
        {
            var adapter = Context.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(channel, uri);
            adapter.Concurrency = concurrency == 0 ? 1 : concurrency;
            adapter.Frequency = frequency == 0 ? 1 : frequency;
            Context.GetComponent<IAdapterRegistry>().RegisterInputChannelAdapter(adapter);
        }

        public void RegisterOutputAdapter(string uri, string channel, TimeSpan scheduled)
        {
            var adapter = Context.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(channel, uri);
            adapter.Interval = scheduled.Seconds;
            Context.GetComponent<IAdapterRegistry>().RegisterOutputChannelAdapter(adapter);
        }

        public void RegisterOutputAdapter(string uri, string channel, int concurrency, int frequency)
        {
            var adapter = Context.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(channel, uri);
            adapter.Concurrency = concurrency == 0 ? 1 : concurrency;
            adapter.Frequency = frequency == 0 ? 1 : frequency;
            Context.GetComponent<IAdapterRegistry>().RegisterOutputChannelAdapter(adapter);
        }

        /// <summary>
        /// This will create a messaging gateway within the infrastructure for sending a message on one 
        /// channel and allowing the infrastructure to process it without a return message on a channel.
        /// </summary>
        /// <typeparam name="TGateway">Type of the gateway</typeparam>
        /// <param name="inputChannel">Name of the input channel</param>
        /// <param name="methodName">Name of the method that will process the message</param>
        /// <returns></returns>
        public TGateway RegisterGateway<TGateway>(string inputChannel, string methodName)
        {
            return this.RegisterGateway<TGateway>(inputChannel, string.Empty, methodName);
        }

        /// <summary>
        /// This will create a messaging gateway within the infrastructure for sending a message on one 
        /// channel and receiving the response on a different channel
        /// </summary>
        /// <typeparam name="TGateway">Type of the gateway</typeparam>
        /// <param name="inputChannel">Name of the input channel</param>
        /// <param name="outputChannel">Name of the output channel</param>
        /// <param name="methodName">Name of the method that will process the message</param>
        /// <returns></returns>
        public TGateway RegisterGateway<TGateway>(string inputChannel, string outputChannel, string methodName)
        {
            if(!typeof(TGateway).IsInterface)
                throw new Exception("The component that is registered for a gateway must be an interface type and contain " +
                    "one method for accepting a message for processing and an optional return message to the client.");

            if(string.IsNullOrEmpty(methodName))
                throw new Exception("The gateway must be configured with a method name for accepting the message from the client.");

            if (string.IsNullOrEmpty(inputChannel))
                throw new Exception("The gateway can not be manually registered without an input channel.");

            // create the channels:
            Context.GetComponent<IChannelRegistry>().RegisterChannel(inputChannel);
            if(!string.IsNullOrEmpty(outputChannel))
                Context.GetComponent<IChannelRegistry>().RegisterChannel(outputChannel);

            // manually register the gateway (each method must be registered on the gateway):
            var gatewayBuilder = new GatewayElementBuilder();
            gatewayBuilder.Kernel = Container.Kernel;

            gatewayBuilder.Build(typeof(TGateway), methodName);

            var gw = this.Context.GetComponent<TGateway>();

            return gw;
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

            Context.GetComponent<IObjectBuilder>().Register(typeof(TComponent).Name, typeof(TComponent));
            var component = Context.GetComponent<TComponent>();

            // add the channels for the message movement:
            Context.GetComponent<IChannelRegistry>().RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                Context.GetComponent<IChannelRegistry>().RegisterChannel(outputChannel);

            // create a message endpoint activator for the component and register it:
            Context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint(inputChannel, outputChannel, string.Empty, component);

            foreach (var activator in Context.GetComponent<IMessageEndpointRegistry>().GetAllItems())
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

            var component = Context.GetComponent<IObjectBuilder>().Resolve(componentId);

            // add the channels for the message movement:
            Context.GetComponent<IChannelRegistry>().RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                Context.GetComponent<IChannelRegistry>().RegisterChannel(outputChannel);

            // create a message endpoint activator for the component and register it:
            Context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint(inputChannel, outputChannel, string.Empty, component);

            foreach (var activator in Context.GetComponent<IMessageEndpointRegistry>().GetAllItems())
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
                                    typeof (TComponent).FullName);

            if (string.IsNullOrEmpty(inputChannel))
                throw new Exception("The component can not be manually registered without an input channel.");

            Context.GetComponent<IObjectBuilder>().Register(instance.GetType().Name, instance, ActivationStyle.AsInstance);
            var component = Context.GetComponent<TComponent>();

            // add the channels for the message movement:
            Context.GetComponent<IChannelRegistry>().RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                Context.GetComponent<IChannelRegistry>().RegisterChannel(outputChannel);

            // create a message endpoint activator for the component and register it:
            Context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint(inputChannel, outputChannel, string.Empty, component);

            foreach (var activator in Context.GetComponent<IMessageEndpointRegistry>().GetAllItems())
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


        private void ComponentBeginInvoke(object sender,  MessageEndpointActivatorBeginInvokeEventArgs eventArgs)
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

        ~BaseMessageConsumerTestFixture()
        {
            if (_component_message_activator != null)
            {
                _component_message_activator.MessageEndpointActivatorBeginInvoke -= ComponentBeginInvoke;
                _component_message_activator.MessageEndpointActivatorEndInvoke -= ComponentEndInvoke;
            }

            this.Container.Dispose();
        }
    }

}