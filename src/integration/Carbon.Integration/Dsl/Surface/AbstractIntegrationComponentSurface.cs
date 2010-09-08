using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Adapter.Strategies.Retry;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Dsl.Surface.Ports;
using Carbon.Integration.Stereotypes.Gateway;
using Carbon.Integration.Stereotypes.Gateway.Configuration;

namespace Carbon.Integration.Dsl.Surface
{
    /// <summary>
    /// The integration surface is the point where the coordination of
    ///  individual components, input and output adapters are constructed
    ///  for application integration scenarios.
    /// </summary>
    public abstract class AbstractIntegrationComponentSurface
    {
        private List<InputPort> _receive_ports = null;
        private List<OutputPort> _send_ports = null;

        /// <summary>
        /// tuple for components (type, input channel, output channel, method name);
        /// </summary>
        private List<Tuple<Type, string, string, string>> _components = null;

        /// <summary>
        /// tuple for gateways (type, input channel, output channel, method name);
        /// </summary>
        private List<Tuple<Type, string, string, string>> _gateways = null;

        /// <summary>
        /// (Read-Write). The name of the surface.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// (Read-Write). Indicator to auto-configuration routine that indicates whether or not the surface is available for processing messages.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// (Read-Only). Flag to indicate whether or not the surface has been configured.
        /// </summary>
        public bool IsConfigured { get; private set; }

        /// <summary>
        /// (Read-Write). Instance of the container for building and retrieving objects.
        /// </summary>
        public IObjectBuilder ObjectBuilder { get; set; }

        /// <summary>
        /// (Read-Only). The current port to forward errors to.
        /// </summary>
        public OutputPort ErrorPort { get; private set; }

        protected AbstractIntegrationComponentSurface(IObjectBuilder builder)
        {
            ObjectBuilder = builder;
            _receive_ports = new List<InputPort>();
            _send_ports = new List<OutputPort>();
            _components = new List<Tuple<Type, string, string, string>>();
            _gateways = new List<Tuple<Type, string, string, string>>();
        }

        /// <summary>
        /// This will return the listing of all receive ports.
        /// </summary>
        /// <returns></returns>
        public InputPort[] ReceivePorts()
        {
            return _receive_ports.ToArray();
        }

        /// <summary>
        /// This wil return the listing of all send ports.
        /// </summary>
        /// <returns></returns>
        public AbstractPort[] SendPorts()
        {
            return _send_ports.ToArray();
        }

        /// <summary>
        /// This will create a zone dedicated for message receipt from the 
        /// defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateReceivePort(AbstractReceivePipeline pipeline, string channel, string uri)
        {
            var port = new InputPort(pipeline, channel, uri);
            this.AddReceivePort(port);
        }

        /// <summary>
        /// This will create a zone dedicated for message receipt from the 
        /// defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateReceivePort(AbstractReceivePipeline pipeline, string channel, string uri, int concurrency, int frequency)
        {
            var port = new InputPort(pipeline, channel, uri, concurrency, frequency);
            this.AddReceivePort(port);
        }

        /// <summary>
        /// This will create a zone dedicated for message receipt from the 
        /// defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateReceivePort(AbstractReceivePipeline pipeline, string channel, string uri, int scheduled)
        {
            var port = new InputPort(pipeline, channel, uri, scheduled);
            this.AddReceivePort(port);
        }

        /// <summary>
        /// This will build a gateway for accepting messages from a client application 
        /// and register the single method for the gateway for accepting messages:
        /// </summary>
        /// <typeparam name="TGateway"></typeparam>
        /// <param name="methodName"></param>
        public void AddGateway<TGateway>(string methodName)
        {
            // manually register the gateway (each method must be registered on the gateway):
            if (!typeof(TGateway).IsInterface)
                throw new Exception("In order to use a gateway for accepting messages, the declaration of the gateway must be in the form of a interface " +
                " with the appropriate request and/or reply channels noted.");

            try
            {
                var channels = this.ExtractGatewayChannels(typeof(TGateway), methodName);

                var gateway = new Tuple<Type, string, string, string>(typeof(TGateway), channels.Item1, channels.Item2,
                                                                      methodName);

                if (!_gateways.Contains(gateway))
                    _gateways.Add(gateway);

            }
            catch
            {

            }

            var gatewayBuilder = new GatewayElementBuilder();
            gatewayBuilder.Build(ObjectBuilder, typeof(TGateway), methodName);
        }

        /// <summary>
        /// This will add a component to be used for message processing 
        /// by extracting it by reference id from the underlying container
        /// </summary>
        /// <param name="id">Identifier of the component in the container.</param>
        public void AddComponent(string id)
        {
            var component = ObjectBuilder.Resolve(id);
            if (component == null) return;

            var channels = this.ExtractChannels(component.GetType());
            this.BuildAndRegisterComponentAsEndpoint(component, channels.Item1, channels.Item2, string.Empty);
        }

        /// <summary>
        /// This will add a component to be used for message processing 
        /// by extracting it by reference id from the underlying container
        /// </summary>
        /// <param name="id">Identifier of the component in the container.</param>
        /// <param name="configuration">Name of the method to call on the component</param>
        public void AddComponent(string id, IIntegrationComponentConfiguration configuration)
        {
            var component = ObjectBuilder.Resolve(id);
            if (component == null) return;

            var channels = this.ExtractChannels(component.GetType());

            if (configuration == null)
            {
                configuration = new IntegrationComponentConfiguration();
                configuration.InputChannel = channels.Item1;
                configuration.OutputChannel = channels.Item2;
            }
            else
            {
                channels = new Tuple<string, string>(configuration.InputChannel, configuration.OutputChannel);
            }

            this.BuildAndRegisterComponentAsEndpoint(component, channels.Item1, channels.Item2, configuration.MethodName);
        }

        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component</typeparam>
        public void AddComponent<TComponent>() where TComponent : class
        {
            ObjectBuilder.Register(typeof(TComponent).Name, typeof(TComponent));
            var component = ObjectBuilder.Resolve<TComponent>();
            if (component == null) return;

            var channels = this.ExtractChannels(component.GetType());
            this.BuildAndRegisterComponentAsEndpoint(component, channels.Item1, channels.Item2, string.Empty);
        }

        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing.
        /// </summary>
        /// <param name="component">Type of the component</param>
        ///  <param name="methodName">Name of the method on the component to process the message (if known)</param>
        public void AddComponent(Type component, string methodName)
        {
            ObjectBuilder.Register(component.Name, component);
            var instance = ObjectBuilder.Resolve(component);
            if (instance == null) return;

            var channels = this.ExtractChannels(component.GetType());


            this.BuildAndRegisterComponentAsEndpoint(instance, channels.Item1, channels.Item2, methodName);
        }

        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing.
        /// </summary>
        /// <param name="component">Type of the component</param>
        ///  <param name="inputChannel">Name of the channel that the message will be placed on and inspected for processing</param>
        ///  <param name="outputChannel">Name of the channel that the message will be place on after process by the component</param>
        public void AddComponent(Type component, string inputChannel, string outputChannel)
        {
            try
            {
                ObjectBuilder.Register(component.Name, component);
            }
            catch (Exception exception)
            {
                // component already registered...
            }

            var instance = ObjectBuilder.Resolve(component);
            if (instance == null) return;

            var channels = this.ExtractChannels(component.GetType());
            this.BuildAndRegisterComponentAsEndpoint(instance, inputChannel, outputChannel, string.Empty);
        }

        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing and listening for messages over 
        /// a given channel.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component</typeparam>
        ///  <param name="inputChannel">Name of the channel that the message will be placed on and inspected for processing</param>
        public void AddComponent<TComponent>(string inputChannel)
        {
            this.AddComponent<TComponent>(inputChannel, string.Empty, string.Empty);
        }

        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing and listening for messages over 
        /// a given channel.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component</typeparam>
        ///  <param name="inputChannel">Name of the channel that the message will be placed on and inspected for processing</param>
        ///  <param name="outputChannel">Name of the channel that the message will be place on after process by the component</param>
        public void AddComponent<TComponent>(string inputChannel, string outputChannel)
        {
            this.AddComponent<TComponent>(inputChannel, outputChannel, string.Empty);
        }


        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing and listening for messages over 
        /// a given channel and sending all method return values to a 
        /// different channel.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component</typeparam>
        ///  <param name="inputChannel">Name of the channel that the message will be placed on and inspected for processing</param>
        ///  <param name="outputChannel">Name of the channel that the message will be place on after process by the component</param>
        ///  <param name="methodName">Name of the method on the component to process the message (if known)</param>
        public void AddComponent<TComponent>(string inputChannel, string outputChannel, string methodName)
        {
            try
            {
                ObjectBuilder.Register(typeof(TComponent).Name, typeof(TComponent));
            }
            catch
            {
                // component already registered;
            }

            var component = ObjectBuilder.Resolve<TComponent>();
            if (component == null) return;

            this.BuildAndRegisterComponentAsEndpoint(component, inputChannel, outputChannel, methodName);
        }

        /// <summary>
        /// This will create a port dedicated for message delivery to the defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateSendPort(AbstractSendPipeline pipeline, OutputPortConfiguration configuration)
        {
            var port = new OutputPort(pipeline, configuration);
            this.AddSendPort(port);
        }

        /// <summary>
        /// This will create a port dedicated for message delivery to the defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateSendPort(AbstractSendPipeline pipeline, string channel, string uri)
        {
            var port = new OutputPort(pipeline, channel, uri);
            this.AddSendPort(port);
        }

        /// <summary>
        /// This will create a port dedicated for message delivery to the defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateSendPort(AbstractSendPipeline pipeline, string channel, string uri,
            int concurrency, int frequency)
        {
            var port = new OutputPort(pipeline, channel, uri, concurrency, frequency);
            this.AddSendPort(port);
        }

        /// <summary>
        /// This will create a port dedicated for message delivery to the defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateSendPort(AbstractSendPipeline pipeline, string channel, string uri, int scheduled)
        {
            var port = new OutputPort(pipeline, channel, uri, scheduled);
            this.AddSendPort(port);
        }

        /// <summary>
        /// This will create a port dedicated for delivering messages that fail 
        /// in processing to a defined location.
        /// </summary>
        /// <returns></returns)
        public void CreateErrorPort(AbstractSendPipeline pipeline, string channel, string uri)
        {
            var port = new OutputPort(pipeline, channel, uri);
            this.ErrorPort = port;
        }

        /// <summary>
        /// This will create a port dedicated for delivering messages that fail 
        /// in processing to a defined location.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="configuration"></param>
        /// <returns></returns)
        public void CreateErrorPort(AbstractSendPipeline pipeline, ErrorOutputPortConfiguration configuration)
        {
            var port = new OutputPort(pipeline, configuration);
            this.ErrorPort = port;
        }

        /// <summary>
        /// This will configure the integration surface based on the receive and send ports, 
        /// components, and error-port definitions.
        /// </summary>
        public void Configure()
        {
            if (!IsAvailable) return;

            if (IsConfigured) return;

            this.BuildReceivePorts();
            this.BuildCollaborations();
            this.BuildSendPorts();
            this.BuildErrorPort();
            this.RegisterAllPorts();

            IsConfigured = true;
        }

        /// <summary>
        /// (User-defined). This is the point where all receive ports can be constructed for 
        /// receiving mesages from a given location and loading them onto a channel for 
        /// processing.
        /// </summary>
        public abstract void BuildReceivePorts();

        /// <summary>
        /// (User-defined). This is the point where all custom components holding 
        /// business logic are defined for processing the message as it comes in 
        /// from a receive port.
        /// </summary>
        public abstract void BuildCollaborations();

        /// <summary>
        /// (User-defined). This is the point where all send ports can be constructed for 
        /// taking messages from a channel and loading them to a physical location 
        /// for further processing (if neccessary).
        /// </summary>
        public abstract void BuildSendPorts();

        /// <summary>
        /// (User-defined). This is the point where all error messages for the integration 
        /// scenario will be sent.
        /// </summary>
        public abstract void BuildErrorPort();

        /// <summary>
        /// This will give a textual description of the configuration for the integration surface.
        /// </summary>
        /// <returns></returns>
        public string Verbalize()
        {
            var builder = new StringBuilder();
            var receive_port_statement = "Data being received from uri '{0}' will be fowarded to channel '{1}'";
            var collaboration_statement = "\tMessages sent over channel '{0}' will be sent to '{1}' and routed to channel '{2}'";
            var send_port_statement = "Data being received from channel '{0}' will be fowarded to uri '{1}'";

            /*
             * Ex:
             * Messages are configured to be received as follows:
             * 
             * Receive Ports:
             *     Data being received from {receive uri } are to be fowarded to channel {channel name}
             *     Data being received from {receive uri } are to be fowarded to channel {channel name}
             *  
             * Gateways:
             *    Gateway {gateway name} is configured to process message {message type} via method {method name} and route it to channel {input channel} and wait for response on channel {output channel}
             *    Gateway {gateway name} is configured to process message {message type} via method {method name} and route it to channel {input channel} and not wait for a response.
             *    
             * Collaborations:
             *     Messages sent over channel {channel} will be sent to {component name} and routed to channel {channel name}
             *     Messages sent over channel {channel} will be sent to {component name} and routed to channel {channel name}
             *   
             * Send Ports:
             *     Data being received from {receive uri } are to be fowarded to channel {channel name}
             *     Data being received from {receive uri } are to be fowarded to channel {channel name}
            */
            using (var stream = new MemoryStream())
            {
                var trace = new System.Diagnostics.TextWriterTraceListener(stream);
                var preamble = "Configuration for Integration Component Surface: " + this.Name;
                var separator = string.Empty;
                foreach (var c in preamble)
                    separator += "=";

                trace.WriteLine(preamble);
                trace.WriteLine(separator);
                trace.IndentSize = 4;

                #region -- build all receive port definitions: --
                trace.IndentLevel = 1;
                trace.WriteLine("Receive Port(s):");

                if (this.ReceivePorts().Length > 0)
                {
                    foreach (var port in this.ReceivePorts())
                    {
                        var text = string.Format(receive_port_statement, port.Uri, port.Channel);
                        trace.IndentLevel = 2;
                        trace.WriteLine(text);

                        if (port.Pipeline != null)
                        {
                            trace.IndentLevel = 3;
                            var pipeline =
                                string.Format(
                                    "A custom receive pipeline of '{0}' will be invoked for every message and apply the following components after the message is received:",
                                    port.Pipeline.Name);
                            trace.WriteLine(pipeline);

                            if (port.Pipeline.PipelineComponents.Count > 0)
                            {
                                trace.IndentLevel = 4;
                                foreach (var component in port.Pipeline.PipelineComponents)
                                {
                                    trace.WriteLine(string.Format("- {0}", component.Name));
                                }
                            }
                        }
                        trace.WriteLine(string.Empty);
                    }
                }
                else
                {
                    trace.WriteLine("No receive ports defined");
                    trace.WriteLine(string.Empty);
                }

                #endregion

                #region -- build the collaboration components --
                trace.IndentLevel = 1;
                trace.WriteLine("Messaging Gateway(s):");

                if (_gateways.Count > 0)
                {
                    foreach (var gateway in _gateways)
                    {
                        trace.IndentLevel = 2;
                        var text = string.Empty;

                        // input channel
                        if (!string.IsNullOrEmpty(gateway.Item2))
                            text = string.Format("Messages sent to input channel '{0}' ", gateway.Item2);

                        // component
                        if (gateway.Item1 != null)
                            text += string.Format("will be marshalled to the infrastructure via '{0}' ",
                                                  gateway.Item1.FullName);

                        // method name
                        if (!string.IsNullOrEmpty(gateway.Item4))
                            text += string.Format("with exposed method '{0}' ", gateway.Item4);

                        // output channel
                        if (!string.IsNullOrEmpty(gateway.Item3))
                            text += string.Format("and subsequently route the response to output channel '{0}' ",
                                                  gateway.Item3);

                        trace.WriteLine(text);
                    }
                    trace.WriteLine(string.Empty);
                }
                else
                {
                    trace.WriteLine("No message gateways defined");
                    trace.WriteLine(string.Empty);
                }
                #endregion

                #region -- build the collaboration components --
                trace.IndentLevel = 1;
                trace.WriteLine("Component Collaboration(s):");

                if (_components.Count > 0)
                {
                    foreach (var component in _components)
                    {
                        trace.IndentLevel = 2;

                        var text = string.Empty;

                        // input channel
                        if (!string.IsNullOrEmpty(component.Item2))
                            text = string.Format("Messages sent to input channel '{0}' ", component.Item2);

                        // component
                        if (component.Item1 != null)
                            text += string.Format("will be sent to '{0}' ", component.Item1.FullName);

                        // method name
                        if (!string.IsNullOrEmpty(component.Item4))
                            text += string.Format("and handled by method '{0}' ", component.Item4);

                        // output channel 
                        if (!string.IsNullOrEmpty(component.Item3))
                            text += string.Format("and subsequently route the response to output channel '{0}'  ",
                                                  component.Item3);
                        trace.WriteLine(text);
                    }
                    trace.WriteLine(string.Empty);
                }
                else
                {
                    trace.WriteLine("No component collaborations defined");
                    trace.WriteLine(string.Empty);
                }
                #endregion

                #region -- build all send port definitions --
                trace.IndentLevel = 1;
                trace.WriteLine("Send Port(s):");

                if (this.SendPorts().Length > 0)
                {
                    foreach (var port in this.SendPorts())
                    {
                        var sendPort = port as OutputPort;

                        var text = string.Format(send_port_statement, sendPort.Channel, sendPort.Uri);
                        trace.IndentLevel = 2;
                        trace.WriteLine(text);

                        if (sendPort.Pipeline != null)
                        {
                            trace.IndentLevel = 3;
                            var pipeline =
                                string.Format(
                                    "A custom send pipeline of '{0}' will be invoked for every message and apply the following components before the message is sent:",
                                    sendPort.Pipeline.Name);
                            trace.WriteLine(pipeline);

                            if (sendPort.Pipeline.PipelineComponents.Count > 0)
                            {
                                trace.IndentLevel = 4;
                                foreach (var component in sendPort.Pipeline.PipelineComponents)
                                {
                                    trace.WriteLine(string.Format("- {0}", component.Name));
                                }
                            }
                        }
                        trace.WriteLine(string.Empty);
                    }
                }
                else
                {
                    trace.WriteLine("No send ports defined");
                    trace.WriteLine(string.Empty);
                }
                #endregion

                trace.WriteLine(string.Empty);
                trace.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                using (TextReader reader = new StreamReader(stream))
                    builder.Append(reader.ReadToEnd());
            }

            return builder.ToString();
        }

        /// <summary>
        /// This will build all of the input and output adapters based on the port definitions 
        /// for sending and receiving messages.
        /// </summary>
        private void RegisterAllPorts()
        {

            if (this.ErrorPort != null)
            {
                this.ErrorPort.ObjectBuilder = this.ObjectBuilder;
                this.ErrorPort.Build();

                if (this.ErrorPort.Port != null)
                    ObjectBuilder.Resolve<IAdapterRegistry>().RegisterOutputChannelAdapter(ErrorPort.Port);
            }

            foreach (var receivePort in _receive_ports)
            {
                receivePort.Build();
                ObjectBuilder.Resolve<IAdapterRegistry>().RegisterInputChannelAdapter(receivePort.Port);
            }

            foreach (var sendPort in _send_ports)
            {
                sendPort.Build();

                // re-direct send port messages that errored to the defined location:
                if (this.ErrorPort != null)
                    if (this.ErrorPort.Port != null)
                        sendPort.Port.RetryStrategy = new RetryStrategy(2, 1, this.ErrorPort.Port.Uri);

                ObjectBuilder.Resolve<IAdapterRegistry>().RegisterOutputChannelAdapter(sendPort.Port);

            }

        }

        private void AddReceivePort(InputPort port)
        {
            if (!this._receive_ports.Contains(port))
            {
                port.ObjectBuilder = this.ObjectBuilder;
                this._receive_ports.Add(port);
            }
        }

        private void AddSendPort(OutputPort port)
        {
            if (!this._send_ports.Contains(port))
            {
                port.ObjectBuilder = this.ObjectBuilder;
                this._send_ports.Add(port);
            }
        }

        private void BuildAndRegisterComponentAsEndpoint(object component,
            string inputChannel, string outputChannel, string methodName)
        {
            var cm = new Tuple<Type, string, string, string>(component.GetType(), inputChannel, outputChannel,
                                                             methodName);

            if (!_components.Contains(cm))
                _components.Add(cm);

            ObjectBuilder.Resolve<IMessageEndpointRegistry>()
                .CreateEndpoint(inputChannel, outputChannel, methodName, component);
        }

        public Tuple<string, string> ExtractGatewayChannels(Type component, string methodName)
        {
            var retval = new Tuple<string, string>(string.Empty, string.Empty);

            if (!component.IsInterface) return retval;

            var method = component.GetMethod(methodName);

            if (method == null) return retval;

            var attributes = method.GetCustomAttributes(typeof(GatewayAttribute), true);

            if (attributes.Length == 0) return retval;

            var attr = attributes[0] as GatewayAttribute;
            retval = new Tuple<string, string>(attr.RequestChannel, attr.ReplyChannel);

            if (string.IsNullOrEmpty(retval.Item1))
                throw new Exception(string.Format("The gateway component {0} was defined as accepting messages for processing but no channels were defined for accepting the message. " +
                    "Please use the {1} annotation on the component in order for it to at least receive a message", component.FullName, typeof(GatewayAttribute).Name));

            return retval;
        }


        private Tuple<string, string> ExtractChannels(Type component)
        {
            Tuple<string, string> retval = null;
            var attributes = new object[] { };

            if (component.IsClass)
            {
                attributes = component.GetCustomAttributes(typeof(MessageEndpointAttribute), true);

                if (attributes.Length == 0) return retval;

                var attr = attributes[0] as MessageEndpointAttribute;
                retval = new Tuple<string, string>(attr.InputChannel, attr.OutputChannel);

                if (string.IsNullOrEmpty(retval.Item1))
                    throw new Exception(string.Format("The component {0} was defined as accepting messages for processing but no channels were defined for accepting the message. " +
                        "Please use the {1} annotation on the component in order for it to at least receive a message", component.FullName, typeof(MessageEndpointAttribute).Name));

            }


            return retval;
        }

        private void BuildChannels(string inputChannel, string outputChannel)
        {
            var registry = ObjectBuilder.Resolve<IChannelRegistry>();

            if (registry.FindChannel(inputChannel) is NullChannel)
                registry.RegisterChannel(inputChannel);

            if (!string.IsNullOrEmpty(outputChannel))
                if (registry.FindChannel(outputChannel) is NullChannel)
                    registry.RegisterChannel(outputChannel);
        }
    }
}