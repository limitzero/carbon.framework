using System;
using Carbon.Core;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.BankingGateway.Components
{
    public class BankQuoteConfirmationToLoanQuoteConfirmationTranslator
        : ICanConsumeAndReturn<BankQuoteConfirmation, LoanQuoteConfirmation>
    {
        public LoanQuoteConfirmation Consume(BankQuoteConfirmation message)
        {
            var confirmation = new LoanQuoteConfirmation();

            if (message.Error != null)
            {
                confirmation.Error = message.Error;
                return confirmation;
            }

            var success = new LoanQuoteAck();
            success.SSN = message.Success.SSN;
            success.InterestRate = message.Success.InterestRate;
            success.LoanAmount = message.Success.LoanAmount;
            success.QuoteId = message.Success.QuoteId;
            confirmation.Success = success;

            return confirmation;
        }
    }
}