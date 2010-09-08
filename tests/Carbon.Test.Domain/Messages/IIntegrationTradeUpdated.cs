namespace Carbon.Test.Domain.Messages
{
    public interface IIntegrationTradeUpdated
    {
        string StockSymbol { get; set; }
        decimal CurrentValue { get; set; }
        decimal PercentIncrease { get; set; }
    }
}