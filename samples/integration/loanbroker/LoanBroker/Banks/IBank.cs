using System;
using Carbon.Core;
using LoanBroker.Messages;

namespace LoanBroker.Banks
{
    /// <summary>
    /// This is the core contract that each bank will implement for determining 
    /// whether or not it can create a quote for the credit and loan information.
    /// In addtion, the bank will send that request back to an address for collecting
    /// all of the quotes and determine the best one for the client.
    /// </summary>
    public interface  IBank
        : ICanConsumeAndReturn<CreateBankQuoteMessage, BankQuoteCreatedMessage>
    {
        /// <summary>
        /// The logical channel in which the bank will be listening for messages.
        /// </summary>
        string InputChannel { get; set; }

        /// <summary>
        /// The name of the bank
        /// </summary>
        string Name { get; set;}

        /// <summary>
        /// The current rate at which lending can be done by the Federal Reserve.
        /// </summary>
        double PrimeRate { get; set; }

        /// <summary>
        /// The rate at which the bank creates a profit for all loans (added to the prime or sub prime rate)
        /// </summary>
        double  RatePremium { get; set;}

        /// <summary>
        /// The length in months of the loan that the bank can extend.
        /// </summary>
        int MaxLoanTerm { get; set;}

    }
}