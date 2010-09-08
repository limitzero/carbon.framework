using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Integration.Stereotypes.Router.Impl.Configuration.Rules;
using Carbon.Core.Channel;

namespace Carbon.Integration.Stereotypes.Router.Impl.Configuration
{
    /// <summary>
    ///  Default class for implementing the routing rules.
    /// </summary>
    public  class DefaultContentBasedRouter : IContentBasedRouter
    {
        private IRoutingRuleBase m_rule_base;
        private IObjectBuilder _object_builder = null;

        public AbstractChannel RequestChannel { get; private set; }
        public AbstractChannel ResultChannel { get; private set; }

        public DefaultContentBasedRouter()
        {
            ResultChannel = new NullChannel();
        }

        public void SetRequestChannel(AbstractChannel channel)
        {
            RequestChannel = channel;
            RequestChannel.MessageSent += Channel_MessageSentForContentBasedRouter;
        }

        public void LoadRuleBase(IRoutingRuleBase ruleBase)
        {
            m_rule_base = ruleBase;
        }

        /// <summary>
        /// This will create an instance of the routing rule base that holds the rules 
        /// that will be applied to the message. Can only be configured inside of the 
        /// <see cref="InitializeContentBasedRouter"/> method.
        /// </summary>
        /// <returns></returns>
        public IRoutingRuleBase CreateRoutingRuleBase()
        {
            return new DefaultRoutingRuleBase(_object_builder.Resolve<IChannelRegistry>());
        }

        /// <summary>
        /// This will evaluate the message based on the rule base and set the channel for sending the message.
        /// </summary>
        public void Evaluate(IEnvelope message)
        {
            m_rule_base.Evaluate(message);

            if (m_rule_base.Channel != null && (!(m_rule_base.Channel is NullChannel)))
            {
                ResultChannel = m_rule_base.Channel;
                ResultChannel.Send(message);
            }

            if (RequestChannel != null && !(RequestChannel is NullChannel))
                RequestChannel.MessageSent -= Channel_MessageSentForContentBasedRouter;
        }

        private void Channel_MessageSentForContentBasedRouter(object sender, ChannelMessageSentEventArgs e)
        {
            // grab the message from the location and route:
            var messageToRoute = RequestChannel.Receive();

            if (!(messageToRoute is NullEnvelope))
                this.Evaluate(messageToRoute);
        }
    }
}