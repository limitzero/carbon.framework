using System;
using System.Collections;
using System.Collections.Generic;
using Carbon.Core;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Integration.Tests.Api.Surface.Ports;
using Castle.MicroKernel;

namespace Carbon.Integration.Tests.Api.Surface
{
    /// <summary>
    /// The integration surface is the point where the coordination of
    ///  individual components, input and output adapters are constructed
    ///  for application integration scenarios.
    /// </summary>
    public abstract class AbstractIntegrationSurface
    {
        private readonly IKernel m_kernel;
        private List<AbstractPort> m_receive_ports = null;
        private List<AbstractPort> m_send_ports = null;

        /// <summary>
        /// (Read-Only). The current port to forward errors to.
        /// </summary>
        public AbstractPort ErrorPort { get; private set; }

        protected AbstractIntegrationSurface(IKernel kernel)
        {
            m_kernel = kernel;
            this.m_receive_ports = new List<AbstractPort>();
            this.m_send_ports = new List<AbstractPort>();
        }

        /// <summary>
        /// This will return the listing of all receive ports.
        /// </summary>
        /// <returns></returns>
        public AbstractPort[] ReceivePorts()
        {
            return m_receive_ports.ToArray();
        }

        /// <summary>
        /// This wil return the listing of all send ports.
        /// </summary>
        /// <returns></returns>
        public AbstractPort[] SendPorts()
        {
            return m_send_ports.ToArray();
        }

        /// <summary>
        /// This will create a zone dedicated for message receipt from the 
        /// defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateReceivePort(string channel, string uri)
        {
            var port = new InputPort(channel, uri);
            this.AddReceivePort(port);
        }

        /// <summary>
        /// This will create a zone dedicated for message receipt from the 
        /// defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateReceivePort(string channel, string uri, int concurrency, int frequency)
        {
            var port = new InputPort(channel, uri, concurrency, frequency);
            this.AddReceivePort(port);
        }

        /// <summary>
        /// This will create a zone dedicated for message receipt from the 
        /// defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateReceivePort(string channel, string uri, int scheduled)
        {
            var port = new InputPort(channel, uri, scheduled);
            this.AddReceivePort(port);
        }

        /// <summary>
        /// This will add a component to be used for message processing 
        /// by extracting it by reference id from the underlying container
        /// </summary>
        /// <param name="id">Identifier of the component in the container.</param>
        public void AddComponent(string id)
        {
            var component = m_kernel.Resolve(id, new Hashtable());
            if(component == null) return;

            var channels = this.ExtractChannels(component.GetType());
            this.BuildAndRegisterComponentAsEndpoint(component, channels.Item1, channels.Item2);
        }

        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component</typeparam>
        public void AddComponent<TComponent>() where TComponent : class
        {
            m_kernel.AddComponent(typeof(TComponent).Name, typeof(TComponent));
            var component = m_kernel.Resolve<TComponent>();
            if (component == null) return;

            var channels = this.ExtractChannels(component.GetType());
            this.BuildAndRegisterComponentAsEndpoint(component, channels.Item1, channels.Item2);
        }

        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing and listening for messages over 
        /// a given channel.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component</typeparam>
        public void AddComponent<TComponent>(string inputchannel)
        {
            this.AddComponent<TComponent>(inputchannel, string.Empty);
        }

        /// <summary>
        /// This will add a component to the container for processing by 
        /// type for message processing and listening for messages over 
        /// a given channel and sending all method return values to a 
        /// different channel.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component</typeparam>
        public void AddComponent<TComponent>(string inputChannel, string outputChannel)
        {
            m_kernel.AddComponent(typeof(TComponent).Name, typeof(TComponent));
            var component = m_kernel.Resolve<TComponent>();
            if (component == null) return;

            this.BuildAndRegisterComponentAsEndpoint(component, inputChannel, outputChannel);
        }

        /// <summary>
        /// This will create a zone dedicated for message delivery to the 
        /// defined location(s).
        /// </summary>
        /// <returns></returns>
        public void CreateSendPort(string channel, string uri)
        {
            var port = new OutputPort(channel, uri);
            this.AddSendPort(port);
        }

        /// <summary>
        /// This will create a port dedicated for delivering messages that fail 
        /// in processing to a defined location.
        /// </summary>
        /// <returns></returns)
        public void CreateErrorPort(string channel, string uri)
        {
            var port = new OutputPort(channel, uri);
            this.ErrorPort = port;
        }

        /// <summary>
        /// This will configure the integration surface based on the receive and send ports, 
        /// components, and error-port definitions.
        /// </summary>
        public  void Configure()
        {
            this.BuildReceivePorts();
            this.BuildCollaborations();
            this.BuildSendPorts();
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

        private void AddReceivePort(InputPort port)
        {
            if (!this.m_receive_ports.Contains(port))
            {
                port.Kernel = this.m_kernel;
                this.m_receive_ports.Add(port);
            }    
        }

        private void AddSendPort(OutputPort port)
        {
            if (!this.m_send_ports.Contains(port))
            {
                port.Kernel = this.m_kernel;
                this.m_send_ports.Add(port);
            }    
        }

        private void BuildAndRegisterComponentAsEndpoint(object component, string inputChannel, string outputChannel)
        {
            var activator = m_kernel.Resolve<IMessageEndpointActivator>();
            activator.ActivationStyle = EndpointActivationStyle.ActivateOnMessageSent;
            activator.SetInputChannel(inputChannel);
            activator.SetInputChannel(outputChannel);
            activator.SetEndpointInstance(component);

            m_kernel.Resolve<IMessageEndpointRegistry>().Register(activator);
        }

        private Tuple<string, string> ExtractChannels(Type component)
        {
            Tuple<string, string> retval = null;

            var attributes = component.GetCustomAttributes(typeof (MessageEndpointAttribute), true);
            if (attributes.Length == 0) return retval;

            var attr = attributes[0] as MessageEndpointAttribute;
            retval = new Tuple<string, string>(attr.InputChannel, attr.OutputChannel);

            return retval;
        }
    }
}