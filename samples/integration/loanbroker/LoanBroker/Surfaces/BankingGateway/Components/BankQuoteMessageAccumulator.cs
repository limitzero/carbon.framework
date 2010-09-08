using Carbon.Core;
using Carbon.Integration.Stereotypes.Accumulator;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.BankingGateway.Components
{
    /// <summary>
    /// This will collect all of the bank quotes from the partner 
    /// banks for the loan quote request that was issued.
    /// </summary>
    public class BankQuoteMessageAccumulator
        : ICanConsume<BankQuoteCreatedMessage>
    {
        //[Accumulator(typeof(BankQuoteMessageAccumulatorStrategy), true, 4)]
        [Accumulate(typeof(BankQuoteCreatedMessage), true, 4, true)]
        public void Consume(BankQuoteCreatedMessage message)
        {

        }
    }
}