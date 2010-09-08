using System;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB.Saga;
using Carbon.ESB.Stereotypes.Conversations;
using Carbon.ESB.Stereotypes.Saga;
using Carbon.Test.Domain.TimeoutMessages;

namespace Carbon.Test.Domain.Sagas
{
    [MessageEndpoint("test_timeout_message_saga")]
    public class TestTimeoutMessageSaga
        : Saga
    {
        private readonly IAdapterMessagingTemplate m_template;

        public TestTimeoutMessageSaga(IAdapterMessagingTemplate template)
        {
            m_template = template;
        }

        [InitiatedBy]
        public void GetStockValue(StockValueQuery message)
        {
            var ticker = GenerateStockQuote(message.StockSymbol);
            //Bus.Publish(this, ticker);
            //Bus.DelayPublication(this, TimeSpan.FromSeconds(5), ticker);
        }

        [OrchestratedBy]
        public void DisplayMicrosoftTicker(MicrosoftStockTicker ticker)
        {
            m_template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                              new Envelope("Received ticker value : " + ticker.GetType().FullName));
        }

        [OrchestratedBy]
        public void DisplayGoogleTicker(GoogleStockTicker ticker)
        {
            m_template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                              new Envelope("Received ticker value : " + ticker.GetType().FullName));
        }

        [OrchestratedBy]
        public void DisplayUnknownTicker(UnknownStockTicker ticker)
        {
            m_template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                              new Envelope("Received ticker value : " + ticker.GetType().FullName));
        }

        private IStockTicker GenerateStockQuote(string stockname)
        {
            IStockTicker ticker = null;

            if (stockname == "MSFT")
            {
                ticker = new MicrosoftStockTicker();
                ticker.StockSymbol = stockname;
                ticker.StockValue = 45.50M;
            }
            else if (stockname == "GOOG")
            {
                ticker = new GoogleStockTicker();
                ticker.StockSymbol = stockname;
                ticker.StockValue = 35.75M;
            }
            else
            {
                ticker = new UnknownStockTicker();
                ticker.StockSymbol = stockname;
                ticker.StockValue = 25.00M;
            }

            return ticker;
        }
    }
}