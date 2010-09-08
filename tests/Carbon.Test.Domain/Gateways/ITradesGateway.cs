using Carbon.Integration.Stereotypes.Gateway;
using Carbon.Test.Domain.Messages;

namespace Carbon.Test.Domain.Gateways
{
    public interface ITradesGateway
    {
        [Gateway("trades", "ibm_trades")] //request-response
        IIntegrationTradeUpdated ProcessTrade(TradeRequest request);
    }
}