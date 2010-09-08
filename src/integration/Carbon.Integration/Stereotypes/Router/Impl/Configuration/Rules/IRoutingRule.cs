using System;
using System.Collections.Generic;
using System.Text;
using Carbon.Core;
using Carbon.Core.Channel;

namespace Carbon.Integration.Stereotypes.Router.Impl.Configuration.Rules
{
    public interface IRoutingRule
    {
        /// <summary>
        /// (Read-Only). The name of the channel to route the message to after the conditions have been examined.
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// (Read-Only). The channel instance to route the message to after the conditions have been examined.
        /// </summary>
        AbstractChannel Channel { get; }

        /// <summary>
        /// This will return a result of evaluating the message against the rules and will set either 
        /// the <see cref="AbstractChannel">channel</see> or channel name to route the message to.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool IsMatch(IEnvelope message);
    }
}