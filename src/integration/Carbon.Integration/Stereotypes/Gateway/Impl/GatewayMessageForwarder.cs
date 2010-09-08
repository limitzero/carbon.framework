using System.Linq;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;

namespace Carbon.Integration.Stereotypes.Gateway.Impl
{
    public class GatewayMessageForwarder : IGatewayMessageForwarder
    {
        private readonly IObjectBuilder _builder;

        public GatewayMessageForwarder(IObjectBuilder builder)
        {
            _builder = builder;
        }

        public void ForwardMessage(IGatewayDefinition definition, object message)
        {
            // find the output adapter that will accept the message:
            var adapterRegistry = _builder.Resolve<IAdapterRegistry>();
            AbstractOutputChannelAdapter outputChannelAdapter = null;

            try
            {
                outputChannelAdapter = (from adapter in adapterRegistry.OutputAdapters
                                        where
                                            adapter.ChannelName.Trim().ToLower() == definition.RequestChannel.Trim().ToLower()
                                        select adapter).First();
            }
            catch
            {

            }

            // HACK: See why this is not working correctly, I think the messages must be put onto an adapter from the gateway
            var envelope = new Envelope(message);

            if (outputChannelAdapter != null)
                outputChannelAdapter.Send(envelope);
            else
            {
                var target = _builder.Resolve<IChannelRegistry>().FindChannel(definition.RequestChannel);
                target.Send(envelope);
            }

        }
    }
}