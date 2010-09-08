using System.Collections.Generic;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Channel;

namespace Carbon.Integration.Stereotypes.Router.Impl.Configuration.Rules
{
    public class DefaultRoutingRuleBase : IRoutingRuleBase
    {
        private readonly IChannelRegistry m_registry;
        private List<IRoutingRule> _rules = null;

        public AbstractChannel Channel { get; private set; }
        public IRoutingRule[] RoutingRules { get; set; }

        public DefaultRoutingRuleBase(IChannelRegistry registry)
        {
            m_registry = registry;
            _rules = new List<IRoutingRule>();
        }

        public void LoadRule<TRoutingRule>() where TRoutingRule : IRoutingRule, new()
        {
            var rule = new TRoutingRule();
            this.AddRoutingRule(rule);
        }

        public void LoadRules(params IRoutingRule[] rules)
        {
            foreach (var rule in rules)
                this.AddRoutingRule(rule);
        }

        public void Evaluate(IEnvelope message)
        {
            Channel = new NullChannel();

            foreach (var routingRule in RoutingRules)
            {
                if (routingRule.IsMatch(message))
                {
                    SetChannelForSend(routingRule, message);
                    break;
                }
            }
        }

        private void SetChannelForSend(IRoutingRule rule, IEnvelope message)
        {
            // if the channel name is given, then we have to look it up
            // for right now, we will re-create the channel based on the 
            // channel name:

            if (!string.IsNullOrEmpty(rule.ChannelName))
            {
                var channel = m_registry.FindChannel(rule.ChannelName);
                Channel = channel;
            }

        }

        private void AddRoutingRule(IRoutingRule rule)
        {
            if(!_rules.Contains(rule))
                _rules.Add(rule);

            RoutingRules = _rules.ToArray();
        }
    }
}