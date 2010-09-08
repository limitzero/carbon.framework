using Carbon.Integration.Stereotypes.Gateway;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.LoanAcceptance.Components
{
    /// <summary>
    /// This is the gateway exposed to the client for requesting a quote on a loan.
    /// </summary>
    public interface ILoanQuoteGateway
    {
        [Gateway("loan.quote.request", "loan.quote.reply")]
        LoanQuoteConfirmation ProcessLoanQuote(LoanQuoteQuery message);
    }
}