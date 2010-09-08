using System.Collections.ObjectModel;
using System.Text;
using Carbon.Integration.Stereotypes.Router.Impl.Configuration.Rules;

namespace Carbon.Integration.Stereotypes.Router.Impl
{
    public interface IRouterStrategy : IChannelStrategy
    {
        /// <summary>
        /// (Read-Only). The name of the channel that is found from the result of the routing rules.
        /// </summary>
        string ResultChannelName { get; }

        /// <summary>
        /// (Read-Only). The list of rules to be applied for routing a message.
        /// </summary>
        ReadOnlyCollection<IRoutingRule> Rules { get; }

        /// <summary>
        /// This will load a series of rules for routing the message:
        /// </summary>
        /// <param name="rules"></param>
        void LoadRules(params IRoutingRule[] rules);

        /// <summary>
        /// This will load a rule for routing a message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void LoadRule<T>() where T : class, IRoutingRule, new();
    }
}