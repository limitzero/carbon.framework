using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Carbon.Integration.Stereotypes.Router.Impl;
using Carbon.Integration.Stereotypes.Router.Impl.Configuration;

namespace Carbon.Integration.Channels.Strategies.Router
{
    public abstract class BaseRouterStrategy : IRouterStrategy
    {
        private IContext m_context = null;
        private List<IRoutingRule> m_routing_rules = null;
        private string m_method_name = string.Empty;

        public event EventHandler<ChannelStrategyCompleteEventArgs> ChannelStrategyCompleted;
        public BaseChannel InputChannel { get; private set; }
        public BaseChannel OutputChannel { get; private set; }
        public MethodInfo CurrentMethod { get; private set; }
        public object CurrentInstance { get; private set; }
        public string ResultChannelName { get; private set; }

        public ReadOnlyCollection<IRoutingRule> Rules { get; private set; }

        protected BaseRouterStrategy()
        {
            m_routing_rules = new List<IRoutingRule>();
            this.OutputChannel = new NullChannel();
        }

        public void SetContext(IContext context)
        {
            m_context = context;
        }

        public void SetInputChannelName(string channelName)
        {
            if (m_context != null)
            {
                var registry = m_context.Resolve<IChannelRegistry>();
                if (registry != null)
                {
                    var channel = registry.FindChannel(channelName);

                    if (!(channel is NullChannel))
                        SetInputChannel(channel);
                }
            }
        }

        public void SetInputChannel(BaseChannel channel)
        {
            InputChannel = channel;
            InputChannel.RegisterActionsOnMessageReceived(RouterInputChannelMessageReceived);
        }

        public void SetOutputChannelName(string channelName)
        {
            if (m_context != null)
            {
                var registry = m_context.Resolve<IChannelRegistry>();
                if (registry != null)
                {
                    var channel = registry.FindChannel(channelName);

                    if (!(channel is NullChannel))
                        SetOutputChannel(channel);
                }
            }
        }

        public void SetOutputChannel(BaseChannel channel)
        {
            OutputChannel = channel;
        }

        public void SetInstance(object instance)
        {
            CurrentInstance = instance;
        }

        public void SetMethod(MethodInfo method)
        {
            CurrentMethod = method;
        }

        public void SetMethodName(string methodName)
        {
            m_method_name = methodName;
        }

        public void ExecuteStrategy(IEnvelope message)
        {
            try
            {
                // check for the method that will be executing the routing strategy (if supplied):
                if (!string.IsNullOrEmpty(this.m_method_name))
                {
                    try
                    {
                        this.CurrentMethod = this.CurrentInstance.GetType().GetMethod(this.m_method_name);
                    }
                    catch (Exception exception)
                    {
                        throw;
                    }
                }

                // implement the routing strategy:
                this.DoRouterStrategy(message);

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
                throw new ChannelStrategyException(input, output, msg, exception);
            }
   
        }

        /// <summary>
        /// This is the custom part of the router strategy where the message
        /// is routed according to user-defined logic. 
        /// </summary>
        /// <param name="message">Message to split.</param>
        public virtual void DoRouterStrategy(IEnvelope message)
        {
            // this is the default routing strategy for a content-based router:
            var contentBasedRouter = m_context.Resolve<IContentBasedRouter>();

            // configure the rule base with the user-defined rules:
            var ruleBase = m_context.Resolve<IRoutingRuleBase>();
            ruleBase.LoadRules(m_routing_rules.ToArray());

            // load the router with the rules:
            contentBasedRouter.LoadRuleBase(ruleBase);

            try
            {
                // evaulate based on rules and send to the channel:
                contentBasedRouter.Evaluate(message);
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
            catch (Exception exception)
            {
                throw;
            }
        }

        public void OnRouterStrategyCompleted(string channel, IEnvelope message)
        {
            EventHandler<ChannelStrategyCompleteEventArgs> evt = this.ChannelStrategyCompleted;
            
            // event-driven portion of router:
            if(evt != null)
                evt(this, new ChannelStrategyCompleteEventArgs(channel, message));
            
            // push the message to the next channel:
            if(!(this.OutputChannel is NullChannel))
                this.OutputChannel.Send(message); 

        }

        public void LoadRules(params IRoutingRule[] rules)
        {
            m_routing_rules.AddRange(rules);
        }

        public void LoadRule<T>() where T : class, IRoutingRule, new()
        {
            var rule = new T();
            if (!m_routing_rules.Contains(rule))
                m_routing_rules.Add(rule);
        }

        private void InputChannel_Router_MsgSent(object sender, ChannelMessageSentEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void InputChannel_Router_MsgRecvd(object sender, ChannelMessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RouterInputChannelMessageReceived(ChannelMessageReceivedEventArgs eventArgs)
        {
            if(!(eventArgs.Message is NullEnvelope))
                this.ExecuteStrategy(eventArgs.Message);
        }



    }
}