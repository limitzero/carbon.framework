using System.Collections.Generic;
using Carbon.Integration.Stereotypes.Accumulator.Impl;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.BankingGateway.Components
{
    /// <summary>
    /// Current strategy for accumulating all bank quote responses.
    /// </summary>
    public class BankQuoteMessageAccumulatorStrategy :
        AbstractAccumulatorMessageHandlingStrategy<BankQuoteCreatedMessage>
    {
        private static IList<BankQuoteCreatedMessage> _items = null;

        public BankQuoteMessageAccumulatorStrategy()
        {
            if(_items == null)
                _items = new List<BankQuoteCreatedMessage>();
            SetStorage(_items);
        }
     
    }
}