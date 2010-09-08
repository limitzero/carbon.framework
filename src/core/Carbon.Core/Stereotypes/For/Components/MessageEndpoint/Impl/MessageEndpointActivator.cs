using System;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Internals;
using Carbon.Core.Internals.Dispatcher;
using Carbon.Core.Internals.MessageResolution;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Core.Channel;

namespace Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl
{
    /// <summary>
    /// The message endpoint activator is the concrete implementation of the EIP of the "message endpoint".
    /// </summary>
    public class MessageEndpointActivator : IMessageEndpointActivator
    {
        private readonly IObjectBuilder m_object_builder;
        private TransactionContext m_transaction_context = null;

        #region -- events --

        /// <summary>
        /// Event that is triggered when the component has started to invoke a method matching the message.
        /// </summary>
        public event EventHandler<MessageEndpointActivatorBeginInvokeEventArgs> MessageEndpointActivatorBeginInvoke;

        /// <summary>
        /// Event that is triggered when the component has finished invoking a method matching the message.
        /// </summary>
        public event EventHandler<MessageEndpointActivatorEndInvokeEventArgs> MessageEndpointActivatorEndInvoke;

        /// <summary>
        /// Event that is triggered when the component has generated an error invoking a method matching the message.
        /// </summary>
        public event EventHandler<MessageEndpointActivatorErrorEventArgs> MessageEndpointActivatorError;

        #endregion

        #region -- properties --
        /// <summary>
        /// (Read-Only). The instance identifier of the endpoint activator.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// (Read-Write). This will determine how the messaging end point will be invoked to process a message.
        /// Operates in <seealso cref="EndpointActivationStyle.ActivateOnMessageSent"/> mode by default.
        /// </summary>
        public EndpointActivationStyle ActivationStyle { get; set; }

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
        public object EndpointInstance { get; private set; }

        /// <summary>
        /// (Read-Only). The type of the instance that will be used to process the message over the input and output channels.
        /// </summary>
        public Type EndpointInstanceType { get; private set; }

        /// <summary>
        /// (Read-Only). The name of the method that has been either set or resolved internally for processing the message on the 
        /// service instance.
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// (Read-Only). The message that is returned from the result of calling the end point.
        /// </summary>
        public IEnvelope ReturnMessage { get; private set; }

        #endregion

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="objectBuilder"></param>
        public MessageEndpointActivator(IObjectBuilder objectBuilder)
        {
            m_object_builder = objectBuilder;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// This will set the input channel for the service activated component for receiving a message.
        /// </summary>
        /// <param name="name">The name of the channel</param>
        public void SetInputChannel(string name)
        {
            var channel = m_object_builder.Resolve<IChannelRegistry>().FindChannel(name);

            if (!(channel is NullChannel))
                SetInputChannel(channel);
        }

        /// <summary>
        /// This will set the input channel for the message activated component for receiving a message.
        /// </summary>
        /// <param name="channel">The channel that will hold the contents for the message to be processed.</param>
        public void SetInputChannel(AbstractChannel channel)
        {
            this.InputChannel = channel;

            if(this.ActivationStyle == EndpointActivationStyle.ActivateOnMessageSent)
                this.InputChannel.MessageSent += OnMessageEndpointInputChannelMessageSent;

            if (this.ActivationStyle == EndpointActivationStyle.ActivateOnMessageReceived)
                this.InputChannel.MessageReceived += OnMessageEndpointInputChannelMessageReceived;

            if(this.ActivationStyle == EndpointActivationStyle.ActivateForBiDirectional)
            {
                this.InputChannel.MessageSent += OnMessageEndpointInputChannelMessageSent;
                this.InputChannel.MessageReceived += OnMessageEndpointInputChannelMessageReceived;
            }

        }

        /// <summary>
        /// This will set the output channel where the message will be delivered after processing.
        /// </summary>
        /// <param name="name"></param>
        public void SetOutputChannel(string name)
        {
            var channel = m_object_builder.Resolve<IChannelRegistry>().FindChannel(name);

            if (!(channel is NullChannel))
                SetOutputChannel(channel);
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
        /// <param name="endpointInstance"></param>
        public void SetEndpointInstance(object endpointInstance)
        {
            this.EndpointInstance = endpointInstance;
            this.EndpointInstanceType = endpointInstance.GetType();
        }

        public void SetEndpointInstance(Type endpoint)
        {
            this.EndpointInstanceType = endpoint;
        }

        /// <summary>
        /// This will set the method that should be invoked for the message on the component 
        /// if known before invocation.
        /// </summary>
        /// <param name="name"></param>
        public void SetEndpointInstanceMethodName(string name)
        {
            this.MethodName = name;
        }

        /// <summary>
        /// This will invoke the endpoint and apply any message handling behavior.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IEnvelope InvokeEndpoint(IEnvelope message)
        {
            IEnvelope result = new NullEnvelope();
            
            try
            {
                if (EndpointInstance == null && EndpointInstanceType == null)
                    throw new Exception(string.Format("No object instance or object type is assigned to the message endpoint activator on channel '{0}',", 
                        this.InputChannel.Name));

                if(EndpointInstance == null)
                    this.EndpointInstance = m_object_builder.Resolve(this.EndpointInstanceType);

                // resolve the method name for invocation:
                MapMessageToMethod messageToMethodResolver = null;

                if (!string.IsNullOrEmpty(this.MethodName))
                {
                    messageToMethodResolver = new MapMessageToMethod(this.MethodName);
                }
                else
                {
                    messageToMethodResolver = new MapMessageToMethod();
                }

                // map the message to the method and invoke:
                var method = messageToMethodResolver.Map(this.EndpointInstance, message);

                if (method == null)
                    throw new ArgumentException(string.Format("The message '{0}' could not be mapped to a method on the service instance '{1}' processed over input channel '{2}'.",
                                                              message.Body.GetPayload<object>().GetType().FullName,
                                                              this.EndpointInstance.GetType().FullName),
                                                              this.InputChannel.Name);

                this.MethodName = method.Name;

                OnBeginInvoke(message);

                var messageHandlingStrategy = IsMessageHandlingStrategyAttachedToMethod(method);

                if (messageHandlingStrategy != null)
                {
                    // execute the message handling strategy for the method taking the current message
                    // as the input to the message handling strategy (return a null message as it has 
                    // already gone out to the output channel via the strategy):
                    ExecuteMethodChannelStrategy(messageHandlingStrategy, method, message);
                    result = new NullEnvelope();
                }
                else
                {
                    // invoke the method on the endpoint that does not have any associated behavior 
                    // attached and forward the result to the output channel (if specified):
                    result = m_object_builder.Resolve<IMessageDispatcher>().Dispatch(this.EndpointInstance, message);

                    OnEndInvoke(result);

                    // forward the message to the next channel (if neccessary):
                    this.ForwardMessage(result);
                }

            }
            catch (Exception exception)
            {
                if (!OnError(message, exception))
                    throw;
            }

            return result;
        }

        /// <summary>
        /// This is the starting point for invoking the endpoint when the message is received/sent 
        /// to the input channel via the event "OnMessageEndpointInputChannelMessageSent".
        /// </summary>
        /// <param name="message"></param>
        private void MonitorInvocation(IEnvelope message)
        {
            this.TriggerEndpointForMessage(message);
        }

        /// <summary>
        /// This will trigger the end point with the message for processing and set
        /// the return message for inspection.
        /// </summary>
        /// <param name="message"></param>
        private void TriggerEndpointForMessage(IEnvelope message)
        {
            var returnedMessage = this.InvokeEndpoint(message);

            if (!(returnedMessage is NullEnvelope) & returnedMessage.Body.Payload != null)
                this.ReturnMessage = returnedMessage;
        }

        /// <summary>
        /// This will forward the message to the output channel (if defined on the component or 
        /// message endpoint activator).
        /// </summary>
        /// <param name="message"></param>
        private void ForwardMessage(IEnvelope message)
        {

            if (this.OutputChannel == null)
                return;

            if (message is NullEnvelope)
                return;

            if (!(this.OutputChannel is NullChannel))
                this.OutputChannel.Send(message);
            else
            {
                var channelName = this.ExtractOutputChannel();
                var channel = m_object_builder.Resolve<IChannelRegistry>().FindChannel(channelName);
                channel.Send(message);
            }

        }

        /// <summary>
        /// This will inspect the attribute on the method to execute to determine
        /// if a channel strategy should take over for the execution of the message
        /// to the method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private IMessageHandlingStrategyAttribute IsMessageHandlingStrategyAttachedToMethod(MethodInfo method)
        {
            IMessageHandlingStrategyAttribute strategyAttribute = null;

            var attrs = method.GetCustomAttributes(true);

            foreach (var attr in attrs)
            {
                if (!typeof(IMessageHandlingStrategyAttribute).IsAssignableFrom(attr.GetType())) continue;
                strategyAttribute = (IMessageHandlingStrategyAttribute)attr;
                break;
            }

            return strategyAttribute;
        }

        /// <summary>
        /// This will execute the indicated channel strategy on the method for the message
        /// being sent over the channel to the component for processing and deliver the resultant
        /// message to the output channel as designated on the component and/or end point.
        /// </summary>
        /// <param name="strategyAttribute"></param>
        /// <param name="method"></param>
        /// <param name="message"></param>
        private void ExecuteMethodChannelStrategy(IMessageHandlingStrategyAttribute strategyAttribute,
                                                  MethodInfo method, IEnvelope message)
        {
            var reflection = m_object_builder.Resolve<IReflection>();
            object strategyInstance = null;
            IMessageHandlingStrategy strategy = null;

            try
            {
                if(strategyAttribute.Strategy.IsGenericType)
                {
                    strategyInstance = reflection.BuildInstance(strategyAttribute.Strategy);
                }
                else
                {
                    strategyInstance = reflection.BuildInstance(strategyAttribute.Strategy.AssemblyQualifiedName);    
                }
                
            }
            catch(Exception exception)
            {
                throw;
            }

            try
            {
                if (strategyInstance != null)
                {
                    strategy = strategyInstance as IMessageHandlingStrategy;
                }

                if (strategy != null)
                {
                    strategy.ChannelStrategyCompleted += MessageEndpointActivator_MessageHandlingStrategyCompleted;
                    strategy.SetContext(m_object_builder);
                    strategy.SetInstance(this.EndpointInstance);
                    strategy.SetMethod(method);
                    strategy.SetOutputChannel(ExtractOutputChannel());
                    strategy.ExecuteStrategy(message);
                }

            }
            catch(Exception exception)
            {
                throw;
            }
            finally
            {
                if (strategy != null)
                    strategy.ChannelStrategyCompleted -= MessageEndpointActivator_MessageHandlingStrategyCompleted;
            }

        }

        /// <summary>
        /// This will extract the output channel as indicated in the 
        /// <see cref="MessageEndpointAttribute">message end point attribute</see>
        /// annotation to the component.
        /// </summary>
        /// <returns></returns>
        private string ExtractOutputChannel()
        {
            var channelName = string.Empty;

            if (this.OutputChannel != null)
                if (!(this.OutputChannel is NullChannel))
                    return this.OutputChannel.Name;

            var attrs =
                this.EndpointInstance.GetType().GetCustomAttributes(
                    typeof(MessageEndpointAttribute), true);

            foreach (var attr in attrs)
            {
                if (attr.GetType() == typeof(MessageEndpointAttribute))
                {
                    channelName = ((MessageEndpointAttribute)attr).OutputChannel;
                    break;
                }
            }

            return channelName;
        }

        private void OnBeginInvoke(IEnvelope message)
        {
            EventHandler<MessageEndpointActivatorBeginInvokeEventArgs> evt = this.MessageEndpointActivatorBeginInvoke;
            if (evt != null)
                evt(this, new MessageEndpointActivatorBeginInvokeEventArgs(this.EndpointInstance, this.MethodName, message));
        }

        private void OnEndInvoke(IEnvelope message)
        {
            EventHandler<MessageEndpointActivatorEndInvokeEventArgs> evt = this.MessageEndpointActivatorEndInvoke;
            if (evt != null)
                evt(this, new MessageEndpointActivatorEndInvokeEventArgs(this.EndpointInstance, this.MethodName, message));
        }

        private bool OnError(IEnvelope message, Exception exception)
        {
            EventHandler<MessageEndpointActivatorErrorEventArgs> evt = this.MessageEndpointActivatorError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new MessageEndpointActivatorErrorEventArgs(this.EndpointInstance, this.MethodName, message, exception));

            return isHandlerAttached;
        }

        private void OnMessageEndpointInputChannelMessageSent(object sender, ChannelMessageSentEventArgs e)
        {
            this.MonitorInvocation(e.Envelope);
        }

        private void OnMessageEndpointInputChannelMessageReceived(object sender, ChannelMessageReceivedEventArgs e)
        {
            this.MonitorInvocation(e.Envelope);
        }

        private void MessageEndpointActivator_MessageHandlingStrategyCompleted(object sender,
                                                                               MessageHandlingStrategyCompleteEventArgs e)
        {
            if(!string.IsNullOrEmpty(e.NextChannel))
            {
                var channel = m_object_builder.Resolve<IChannelRegistry>().FindChannel(e.NextChannel);

                if(channel is NullChannel)
                    throw new Exception("The channel '" + e.NextChannel + "' was not found to deliver the message '" +  e.Message.Body.GetPayload<object>().GetType().FullName + "' to.");

                channel.Send(e.Message);
            }

            OnEndInvoke(e.Message);
        }

        ~MessageEndpointActivator()
        {
            if (this.InputChannel == null)
                return;

            this.InputChannel.MessageSent -= OnMessageEndpointInputChannelMessageSent;
            this.InputChannel.MessageReceived -= OnMessageEndpointInputChannelMessageReceived;
        }
    }
}