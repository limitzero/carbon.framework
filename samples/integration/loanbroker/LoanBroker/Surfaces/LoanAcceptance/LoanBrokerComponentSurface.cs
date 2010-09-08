using System;
using Carbon.Core.Builder;
using Carbon.Integration.Dsl.Surface;
using LoanBroker.Surfaces.LoanAcceptance.Components;

namespace LoanBroker.Surfaces.LoanAcceptance
{
    public class LoanBrokerComponentSurface
        : AbstractIntegrationComponentSurface
    {
        public const string LOAN_QUOTE_INPUT_CHANNEL = "loan.quote.request";
        public const string LOAN_QUOTE_OUTPUT_CHANNEL = "loan.quote.reply";

        public LoanBrokerComponentSurface(IObjectBuilder builder) : base(builder)
        {
            Name = "Loan Broker Component Surface";
            IsAvailable = true;
        }

        public override void BuildReceivePorts()
        {
            
        }

        public override void BuildCollaborations()
        {
            // create the gateway for the client to submit loan quotes:
            AddGateway<ILoanQuoteGateway>("ProcessLoanQuote");   

            // create the component to take the loan request and create a credit inquiry:
            AddComponent<LoanQuoteMessageConsumer>(LOAN_QUOTE_INPUT_CHANNEL, 
                CreditBureau.CreditBureauComponentSurface.CREDIT_BUREAU_INPUT_CHANNEL);
        }

        public override void BuildSendPorts()
        {
          
        }

        public override void BuildErrorPort()
        {
          
        }
    }
}