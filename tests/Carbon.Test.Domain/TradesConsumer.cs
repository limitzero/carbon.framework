using System;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Stereotypes.Consumes;
using Carbon.Test.Domain.Messages;
using Carbon.Core.Builder;

namespace Carbon.Test.Domain
{
    [MessageEndpoint("trades")]
    public class TradesConsumer
    {
        private readonly IObjectBuilder m_builder;
        private readonly IAdapterMessagingTemplate m_template;

        public TradesConsumer(
            IObjectBuilder builder,
            IAdapterMessagingTemplate template)
        {
            m_template = template;
            m_builder = builder;
        }

        /// <summary>
        /// This will take the trade request and return back 
        /// an IBM trade on the channel "ibm_trades".
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Consumes("ibm_trades")]
        public IIBMTradeUpdated AcceptTradeRequest(TradeRequest message)
        {
            m_template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                              new Envelope("Received trade request..."));
            var trade = m_builder.CreateComponent<IIBMTradeUpdated>();
            return trade;
        }
    }
}