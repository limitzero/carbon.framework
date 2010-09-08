using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Internals.MessageResolution;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Integration.Stereotypes.Router.Impl.Configuration;
using Carbon.Integration.Stereotypes.Router.Impl.Configuration.Rules;
using Carbon.Core.Internals.Reflection;

namespace Carbon.Integration.Stereotypes.Router.Impl
{
    public abstract class AbstractRouterMessageHandlingStrategy : IRouterMessageHandlingStrategy
    {
        private IObjectBuilder m_object_builder = null;
        private List<IRoutingRule> _routing_rules;

        public string ResultChannelName { get; private set; }

        public ReadOnlyCollection<IRoutingRule> Rules { get; private set; }

        /// <summary>
        /// Event that is triggered when the strategy has been completed.
        /// </summary>
        public event EventHandler<MessageHandlingStrategyCompleteEventArgs> ChannelStrategyCompleted;

        /// <summary>
        /// (Read-Only). The channel that the source message will be produced on for 
        /// compiling into one message for delivery to the output channel.
        /// </summary>
        public AbstractChannel InputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The channel that the reconstructed message will be produced 
        /// for subsequent processing.
        /// </summary>
        public AbstractChannel OutputChannel { get; private set; }

        /// <summary>
        /// (Read-Only). The current method that is invoked to implement the channel strategy.
        /// </summary>
        public MethodInfo CurrentMethod { get; private set; }

        /// <summary>
        /// (Read-Only). The current object instance where the method is being invoked for the channel strategy.
        /// </summary>
        public object CurrentInstance { get; private set; }

        protected AbstractRouterMessageHandlingStrategy()
        {
            _routing_rules = new List<IRoutingRule>();
            this.OutputChannel = new NullChannel();
        }

        /// <summary>
        /// This will set the corresponding context for the adapter to access any resources
        /// that it may need via the underlying object container.
        /// </summary>
        /// <param name="objectBuilder"></param>
        public void SetContext(IObjectBuilder objectBuilder)
        {
            m_object_builder = objectBuilder;
        }

        /// <summary>
        /// This will set the channel, by name, that the channel strategy will listen on for messages.
        /// </summary>
        /// <param name="channelName">Name of the input channel.</param>
        public void SetInputChannel(string channelName)
        {
            if (m_object_builder != null)
            {
                var registry = m_object_builder.Resolve<IChannelRegistry>();
                if (registry != null)
                {
                    var channel = registry.FindChannel(channelName);

                    if (!(channel is NullChannel))
                        SetInputChannel(channel);
                }
            }
        }

        /// <summary>
        /// This will set the channel that the channel strategy will listen on for messages.
        /// </summary>
        /// <param name="channel">Input channel for individual source message.</param>
        public void SetInputChannel(AbstractChannel channel)
        {
            InputChannel = channel;
            InputChannel.MessageSent += InputChannel_Aggregator_MsgSent;
        }

        /// <summary>
        /// This will set the channel, by name, that the channel strategy will deliver the message to after processing.
        /// </summary>
        /// <param name="channelName">Name of the output channel.</param>
        public void SetOutputChannel(string channelName)
        {
            if (m_object_builder != null)
            {
                var registry = m_object_builder.Resolve<IChannelRegistry>();
                if (registry != null)
                {
                    var channel = registry.FindChannel(channelName);

                    if (!(channel is NullChannel))
                        SetOutputChannel(channel);
                }
            }
        }

        /// <summary>
        /// This will set the channel that the channel strategy will deliver the message to after processing.
        /// </summary>
        /// <param name="channel">Output channel for the individual composed message.</param>
        public void SetOutputChannel(AbstractChannel channel)
        {
            OutputChannel = channel;
        }

        /// <summary>
        /// This will set the value of the current instance where the method that is implementing the channel strategy is located.
        /// </summary>
        /// <param name="instance"></param>
        public void SetInstance(object instance)
        {
            CurrentInstance = instance;
        }

        /// <summary>
        /// This will set the value of the current method that is implementing the channel strategy.
        /// </summary>
        /// <param name="method"></param>
        public void SetMethod(MethodInfo method)
        {
            CurrentMethod = method;
        }

        /// <summary>
        /// This will set the value of the current method that is implementing the channel strategy.
        /// </summary>
        /// <param name="name">Name of the method on the instance that will be executing the strategy.</param>
        public void SetMethod(string name)
        {
            if (this.CurrentInstance != null)
                this.CurrentMethod = this.CurrentInstance.GetType().GetMethod(name);
        }

        /// <summary>
        /// This is the custom part of the router strategy where the message
        /// is routed according to user-defined logic. 
        /// </summary>
        /// <param name="message">Message to split.</param>
        public virtual void DoRouterStrategy(IEnvelope message)
        {
            // this is the default routing strategy for a content-based router:
            try
            {
                m_object_builder.Register(typeof(IContentBasedRouter).Name, typeof(IContentBasedRouter), typeof(DefaultContentBasedRouter));
                m_object_builder.Register(typeof(IRoutingRuleBase).Name, typeof(IRoutingRuleBase), typeof(DefaultRoutingRuleBase));
            }
            catch
            {
                // already registered...    
            }

            var contentBasedRouter = m_object_builder.Resolve<IContentBasedRouter>();

            // configure the rule base with the user-defined rules:
            var ruleBase = m_object_builder.Resolve<IRoutingRuleBase>();
            ruleBase.LoadRules(_routing_rules.ToArray());

            // load the router with the rules:
            contentBasedRouter.LoadRuleBase(ruleBase);

            try
            {
                // evaulate based on rules and send to the channel:
                contentBasedRouter.Evaluate(message);

                if(contentBasedRouter.ResultChannel is NullChannel && this.OutputChannel is NullChannel)
                    throw new Exception("The message passed to the content router could not be matched to any rules and there is not an output channel defined on '" + 
                     this.CurrentInstance.GetType().FullName + "' for routing the message for inspection or recovery.");

                if (contentBasedRouter.ResultChannel is NullChannel && !(this.OutputChannel is NullChannel))
                {
                    this.OutputChannel.Send(message);
                    OnRouterStrategyCompleted(string.Empty, message);
                }
                else
                {
                    this.ResultChannelName = contentBasedRouter.ResultChannel.Name;

                    // if the message does not match any rules, send to the configured
                    // output channel that is on the component message end point annotation:
                    if (contentBasedRouter.ResultChannel is NullChannel || contentBasedRouter.ResultChannel == null)
                    {
                        OnRouterStrategyCompleted(string.Empty, message);

                        //if(this.OutputChannel != null & !(this.OutputChannel is NullChannel))
                        //    this.OutputChannel.CreateProducer().Send(message);
                    }
                }
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        /// <summary>
        /// This will execute the custom strategy for the message on the channel.
        /// </summary>
        /// <param name="message"></param>
        public void ExecuteStrategy(IEnvelope message)
        {
            var destination = string.Empty;

            try
            {
                // check for the method that will be executing the routing strategy (if supplied):
                if (CurrentMethod == null)
                {
                    var mapper = new MapMessageToMethod();
                    var method = mapper.Map(this.CurrentInstance, message);
                    this.CurrentMethod = method;
                }

                DoRouterStrategy(message);

                // signal that the message handling strategy is completed:
                OnRouterStrategyCompleted(destination, message);

            }
            catch (Exception exception)
            {
                var input = this.InputChannel.Name;
                var output = !(this.OutputChannel is NullChannel) ? this.OutputChannel.Name : string.Empty;

                var msg =
                string.Format(
                    "An error has occurred while attempting the router strategy for component {0} and method {1}. Reason: {2}",
                    this.CurrentInstance.GetType().FullName,
                    this.CurrentMethod.Name,
                    exception.Message);
                throw new Exception(msg, exception);
            }
        }

        public void OnRouterStrategyCompleted(string channel, IEnvelope message)
        {
            EventHandler<MessageHandlingStrategyCompleteEventArgs> evt = this.ChannelStrategyCompleted;

            // event-driven portion of router:
            if (evt != null)
                evt(this, new MessageHandlingStrategyCompleteEventArgs(channel, message));

            // push the message to the next channel:
            if (!(this.OutputChannel is NullChannel))
                this.OutputChannel.Send(message);

        }

        public void LoadRules(params IRoutingRule[] rules)
        {
            _routing_rules.AddRange(rules);
        }

        public void LoadRule<T>() where T : class, IRoutingRule, new()
        {
            var rule = new T();
            if (!_routing_rules.Contains(rule))
                _routing_rules.Add(rule);
        }

        private void InputChannel_Aggregator_MsgSent(object sender, ChannelMessageSentEventArgs e)
        {
            var message = e.Envelope;
            if (!(message is NullEnvelope))
                this.ExecuteStrategy(message);
        }
    }
}