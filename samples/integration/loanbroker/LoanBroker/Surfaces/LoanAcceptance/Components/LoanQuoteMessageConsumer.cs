using Carbon.Core;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.LoanAcceptance.Components
{
    /// <summary>
    ///  The loan quote message consumer is the endpoint 
    /// that will accept the loan quote request message 
    /// from the client for generating a loan quote 
    ///  reply (delivered over a different channel).
    /// </summary>
    public class LoanQuoteMessageConsumer
        : ICanConsumeAndReturn<LoanQuoteQuery, CreditBureauInquiry>
    {
        public CreditBureauInquiry Consume(LoanQuoteQuery message)
        {
            var creditInquiry = new CreditBureauInquiry
                                    {
                                       SSN = message.SSN, 
                                       LoanAmount = message.LoanAmount,
                                       LoanTerm =  message.LoanTerm
                                    };

            return creditInquiry;
        }
    }
}