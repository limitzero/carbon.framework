using System;
using Carbon.Channel.Registry;
using Carbon.Core.Channel;
using Carbon.Core.Internals.MessageResolution;

namespace Carbon.Core.Stereotypes.For.Components.Service.Impl
{
    /// <summary>
    /// The service activator is the concrete implementation of the EIP of the "Service Activator".
    /// </summary>
    public class ServiceActivator : IServiceActivator
    {
        private readonly IChannelRegistry m_registry;

        #region -- events --

        /// <summary>
        /// Event that is triggered when the component has started to invoke a method matching the message.
        /// </summary>
        public event EventHandler<ServiceActivatorBeginInvokeEventArgs> ServiceActivatorBeginInvoke;

        /// <summary>
        /// Event that is triggered when the component has finished invoking a method matching the message.
        /// </summary>
        public event EventHandler<ServiceActivatorEndInvokeEventArgs> ServiceActivatorEndInvoke;

        /// <summary>
        /// Event that is triggered when the component has generated an error invoking a method matching the message.
        /// </summary>
        public event EventHandler<ServiceActivatorErrorEventArgs> ServiceActivatorError;

        #endregion

        #region -- properties --

        /// <summary>
        /// (Read-Only). The channel by which the message will be delivered to the component for processing.
        /// </summary>
        public AbstractChannel InputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The channel by which the processed message will be delivered.
        /// </summary>
        public AbstractChannel OutputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The instance of the object that will be used to process the message over the input and output channels.
        /// </summary>
        public object ServiceInstance { get; private set; }

        /// <summary>
        /// (Read-Only). The name of the method that has been either set or resolved internally for processing the message on the 
        /// service instance.
        /// </summary>
        public string MethodName { get; private set; }

        #endregion

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="registry"></param>
        public ServiceActivator(IChannelRegistry registry)
        {
            m_registry = registry;
        }

        /// <summary>
        /// This will set the input channel for the service activated component for receiving a message.
        /// </summary>
        /// <param name="name">The name of the channel</param>
        public void SetInputChannel(string name)
        {
            var channel = m_registry.FindChannel(name);

            //TODO: Need some validation here for non-resolution by name:
            this.SetInputChannel(channel);
        }

        /// <summary>
        /// This will set the input channel for the service activated component for receiving a message.
        /// </summary>
        /// <param name="channel">The channel that will hold the contents for the message to be processed.</param>
        public void SetInputChannel(AbstractChannel channel)
        {
            this.InputChannel = channel;
            this.InputChannel.MessageSent += OnInputChannelMessageSent;
        }

        /// <summary>
        /// This will set the output channel where the message will be delivered after processing.
        /// </summary>
        /// <param name="name"></param>
        public void SetOutputChannel(string name)
        {
            var channel = m_registry.FindChannel(name);

            //TODO: Need some validation here for non-resolution by name:

            this.SetOutputChannel(channel);
        }

        /// <summary>
        /// This will set the output channel where the message will be delivered after processing.
        /// </summary>
        /// <param name="channel">The channel that will hold the contents of the processed message.</param>
        public void SetOutputChannel(AbstractChannel channel)
        {
            this.OutputChannel = channel;
        }

        /// <summary>
        /// This will set the activated instance of the component for processing the  
        /// message over the input and output channels.
        /// </summary>
        /// <param name="serviceInstance"></param>
        public void SetServiceInstance(object serviceInstance)
        {
            this.ServiceInstance = serviceInstance;
        }

        /// <summary>
        /// This will set the method that should be invoked for the message on the component 
        /// if known before invocation.
        /// </summary>
        /// <param name="name"></param>
        public void SetServiceInstanceMethodName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                this.MethodName = name;
        }

        private void InvokeService(IEnvelope message)
        {

            try
            {
                // resolve the method name for invocation:
                MapMessageToMethod messageToMethodResolver = null;

                if(!string.IsNullOrEmpty(this.MethodName))
                {
                    messageToMethodResolver = new MapMessageToMethod(this.MethodName);
                }
                else
                {
                    messageToMethodResolver = new MapMessageToMethod();
                }

                // map the message to the method and invoke:
                var method = messageToMethodResolver.Map(this.ServiceInstance, message);

                if (method == null)
                    throw new ArgumentException(string.Format("The message '{0}' could not be mapped to a method on the service instance '{1}'",
                                                              message.Body.GetPayload<object>().GetType().FullName,
                                                              this.ServiceInstance.GetType().FullName));

                this.MethodName = method.Name;

                OnBeginInvoke(message);

                var methodInvoker = new MappedMessageToMethodInvoker(this.ServiceInstance, method);
                var result = methodInvoker.Invoke(message);

                OnEndInvoke(result);

                // forward the message to the next channel (if neccessary):
                this.ForwardMessage(result);

            }
            catch (Exception exception)
            {
                if (!OnError(message, exception))
                    throw;
            }
        }

        private void ForwardMessage(IEnvelope message)
        {
            if (this.OutputChannel == null)
                return;

            if (message is NullEnvelope)
                return;

            this.OutputChannel.Send(message);
        }

        private void OnBeginInvoke(IEnvelope message)
        {
            EventHandler<ServiceActivatorBeginInvokeEventArgs> evt = this.ServiceActivatorBeginInvoke;
            if (evt != null)
                evt(this, new ServiceActivatorBeginInvokeEventArgs(this.ServiceInstance, this.MethodName, message));
        }

        private void OnEndInvoke(IEnvelope message)
        {
            EventHandler<ServiceActivatorEndInvokeEventArgs> evt = this.ServiceActivatorEndInvoke;
            if (evt != null)
                evt(this, new ServiceActivatorEndInvokeEventArgs(this.ServiceInstance, this.MethodName, message));
        }

        private bool OnError(IEnvelope message, Exception exception)
        {
            EventHandler<ServiceActivatorErrorEventArgs> evt = this.ServiceActivatorError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new ServiceActivatorErrorEventArgs(this.ServiceInstance, this.MethodName, message, exception));

            return isHandlerAttached;
        }

        private void OnInputChannelMessageSent(object sender, ChannelMessageSentEventArgs e)
        {
            this.InvokeService(e.Envelope);
        }

        ~ServiceActivator()
        {
            if (this.InputChannel == null)
                return;

            this.InputChannel.MessageSent -= OnInputChannelMessageSent;

        }
    }
}