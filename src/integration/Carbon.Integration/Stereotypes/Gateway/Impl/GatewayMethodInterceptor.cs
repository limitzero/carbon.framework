using System.Linq;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Castle.Core.Interceptor;

namespace Carbon.Integration.Stereotypes.Gateway.Impl
{
    /// <summary>
    /// This is the method inteceptor for all gateway contracts.
    /// </summary>
    public class GatewayMethodInterceptor : IInterceptor
    {
        private readonly IObjectBuilder m_container;
        private readonly IGatewayDefinition m_gateway_definition;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="container"></param>
        /// <param name="gatewayDefinition"></param>
        public GatewayMethodInterceptor(IObjectBuilder container, IGatewayDefinition gatewayDefinition)
        {
            m_container = container;
            m_gateway_definition = gatewayDefinition;
        }

        public void Intercept(IInvocation invocation)
        {
            object result = null;

            if (invocation.Method.Name.Trim().ToLower() == m_gateway_definition.Method.Trim().ToLower())
            {
                this.GetChannels(invocation);
                this.ForwardToChannel(m_gateway_definition,  invocation.Arguments);

                if (invocation.Method.ReturnType != typeof(void))
                {
                    var replyChannel = m_container.Resolve<IChannelRegistry>()
                        .FindChannel(m_gateway_definition.ReplyChannel);

                    if (!(replyChannel is NullChannel))
                    {
                        IEnvelope message = new NullEnvelope();

                        if (m_gateway_definition.ReceiveTimeout != 0)
                            message = replyChannel.Receive(m_gateway_definition.ReceiveTimeout);
                        else
                        {
                            // note: default poll for five minutes (in seconds) for a response on gateway
                            message = replyChannel.Receive();
                        }

                        if (!(message is NullEnvelope))
                            result = message.Body.GetPayload<object>();
                    }
                }

                invocation.ReturnValue = result;
            }

        }

        private void GetChannels(IInvocation invocation)
        {
            var attributes = invocation.Method.GetCustomAttributes(typeof(GatewayAttribute), true);

            if (attributes.Length > 0)
            {
                var attribute = attributes[0] as GatewayAttribute;

                if (string.IsNullOrEmpty(m_gateway_definition.RequestChannel))
                    m_gateway_definition.RequestChannel = attribute.RequestChannel;

                if (string.IsNullOrEmpty(m_gateway_definition.RequestChannel))
                    m_gateway_definition.ReplyChannel = attribute.ReplyChannel;

                if (!string.IsNullOrEmpty(m_gateway_definition.RequestChannel))
                    m_container.Resolve<IChannelRegistry>().RegisterChannel(m_gateway_definition.RequestChannel);

                if (!string.IsNullOrEmpty(m_gateway_definition.ReplyChannel))
                    m_container.Resolve<IChannelRegistry>().RegisterChannel(m_gateway_definition.ReplyChannel);
            }
        }

        private void ForwardToChannel(IGatewayDefinition definition,  object[] arguments)
        {
            var requestChannel = m_container.Resolve<IChannelRegistry>().FindChannel(definition.RequestChannel);

            if ((requestChannel is NullChannel)) return;

            var forwarder = m_container.Resolve<IGatewayMessageForwarder>();

            forwarder.ForwardMessage(definition, arguments[0]);
        }
    }
}