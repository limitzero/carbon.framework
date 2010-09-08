using System;
using Carbon.ESB.Saga;
using Carbon.Core.Stereotypes.For.Components.Message;
namespace Carbon.Test.Domain.TimeoutMessages
{
    [Message]
    public class StockValueQuery : ISagaMessage
    {
        public Guid SagaId { get; set; }
        public string StockSymbol { get; set; }
    }

    public interface IStockTicker : ISagaMessage
    {
        string StockSymbol { get; set; }
        decimal StockValue { get; set; }
    }

    [Message]
    public class MicrosoftStockTicker : IStockTicker
    {
        public Guid SagaId { get; set; }
        public string StockSymbol { get; set; }
        public decimal StockValue { get; set; }
    }
    
    [Message]
    public class GoogleStockTicker : IStockTicker
    {
        public Guid SagaId { get; set; }
        public string StockSymbol { get; set; }
        public decimal StockValue { get; set; }
    }

    [Message]
    public class UnknownStockTicker : IStockTicker
    {
        public Guid SagaId { get; set; }
        public string StockSymbol { get; set; }
        public decimal StockValue { get; set; }
    }
}