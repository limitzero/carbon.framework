using Carbon.Core;
using Carbon.Core.Channel;
using Carbon.Integration.Stereotypes.Router.Impl.Configuration.Rules;

namespace Carbon.Integration.Stereotypes.Router.Impl.Configuration
{
    public interface IContentBasedRouter
    {
        /// <summary>
        /// (Read-Only). The initial channel that the message will be sent to apply the routing rules.
        /// </summary>
        AbstractChannel RequestChannel { get; }

        /// <summary>
        /// (Read-Only). The channel that the message will be sent to after the routing rules are applied.
        /// </summary>
        AbstractChannel ResultChannel { get; }

        /// <summary>
        /// This will set the rule base to evaluate the message against.
        /// </summary>
        void LoadRuleBase(IRoutingRuleBase ruleBase);

        /// <summary>
        /// This will evaluate the message based on the rule base and set the channel for sending the message.
        /// </summary>
        void Evaluate(IEnvelope message);

        /// <summary>
        /// This will set the channel that the message will be produced on to the content based router
        /// for invoking the rules for routing.
        /// </summary>
        /// <param name="channel"></param>
        void SetRequestChannel(AbstractChannel channel);

        IRoutingRuleBase CreateRoutingRuleBase();
    }
}