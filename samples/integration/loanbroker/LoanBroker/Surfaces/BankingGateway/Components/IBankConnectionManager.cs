using System;
using Carbon.Core;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.BankingGateway.Components
{
    /// <summary>
    /// The bank connection manager is responsible for taking 
    /// the current credit bureau information and forwarding 
    /// it to the list of banks that we can partner with for 
    /// generating a quote for accepting the loan.
    /// </summary>
    public interface IBankConnectionManager
        : ICanConsume<CreditBureauReply>
    {
        /// <summary>
        /// (Read-Write). The address that all banks should send their quotes to for the loan request.
        /// </summary>
        string BankQuoteReplyAddress { get; set; }

        /// <summary>
        /// (Read-Write). The listing of banks that can participate in generating a
        /// quote for the loan request based on the credit bureau information.
        /// </summary>
        string[] BankingPartners { get; set;}
    }
}