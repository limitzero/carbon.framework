using Carbon.ESB.Saga;

namespace Carbon.Test.Domain.Messages
{
    public interface ITradeUpdated : ISagaMessage
    {
        string StockSymbol { get; set; }
        decimal CurrentValue { get; set; }
        decimal PercentIncrease { get; set; }
    }
}