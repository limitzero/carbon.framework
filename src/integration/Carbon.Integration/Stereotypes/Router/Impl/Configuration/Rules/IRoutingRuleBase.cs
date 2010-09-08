using Carbon.Core;
using Carbon.Core.Channel;

namespace Carbon.Integration.Stereotypes.Router.Impl.Configuration.Rules
{
    public interface IRoutingRuleBase
    {
        /// <summary>
        /// (Read-Only). The initial channel that the message will be sent to apply the routing rules.
        /// </summary>
        AbstractChannel Channel { get; }

        /// <summary>
        /// (Read-Write). The series of <seealso cref="IRoutingRule">routing rules</seealso>that will 
        /// be examined for the message to determine the route to take.
        /// </summary>
        IRoutingRule[] RoutingRules { get; set; }

        /// <summary>
        /// (Read-Write). The  <seealso cref="IRoutingRule">routing rule</seealso>that will 
        /// be examined for the message to determine the route to take.
        /// </summary>
        void LoadRule<TRoutingRule>() where TRoutingRule : IRoutingRule, new();

        /// <summary>
        /// This will load a series of routing rules into the rule base.
        /// </summary>
        /// <param name="rules"></param>
        void LoadRules(params IRoutingRule[] rules);

        /// <summary>
        /// This will evaluate a message against a set rules and will set either 
        /// the <see cref="AbstractChannel">channel</see> or channel name to route the message to.
        /// </summary>
        /// <param name="message">Message to apply the routing rules to.</param>
        void Evaluate(IEnvelope message);
    }
}