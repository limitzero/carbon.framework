using System;

namespace Carbon.Integration.Stereotypes.Gateway.Impl
{
    public interface IGatewayMessageForwarder
    {
        void ForwardMessage(IGatewayDefinition definition, object message);
    }
}