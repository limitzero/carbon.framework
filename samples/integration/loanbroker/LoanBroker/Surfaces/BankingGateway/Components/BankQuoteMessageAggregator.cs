using Carbon.Core;
using LoanBroker.Messages;
using Carbon.Integration.Stereotypes.Aggregator;

namespace LoanBroker.Surfaces.BankingGateway.Components
{
    /// <summary>
    /// This will aggregate all of the bank quotes into one message 
    /// to send to the loan quote translator for transmitting the 
    /// response back to the client.
    /// </summary>
    public class BankQuoteMessageAggregator : 
        ICanConsume<BankQuoteCreatedMessage[]>
    {
        [Aggregator(typeof(BankQuoteMessageAggregatorStrategy))]
        public void Consume(BankQuoteCreatedMessage[] messages)
        {

        }
    }
}