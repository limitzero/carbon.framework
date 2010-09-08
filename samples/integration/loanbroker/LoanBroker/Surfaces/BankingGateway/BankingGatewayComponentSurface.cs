using System;
using Carbon.Core.Builder;
using Carbon.Integration.Dsl.Surface;
using LoanBroker.Banks;
using LoanBroker.Surfaces.BankingGateway.Components;

namespace LoanBroker.Surfaces.BankingGateway
{
    public class BankingGatewayComponentSurface
        : AbstractIntegrationComponentSurface
    {
        public const string BANK_QUOTE_ACCUMULATOR_INPUT_CHANNEL = "bank.quote.replies";
        public const string BANK_QUOTE_AGGREGATOR_INPUT_CHANNEL = "bank.quote.aggregator";
        public const string BANK_QUOTE_TRANSLATOR_INPUT_CHANNEL = "bank.quote.translator";

        public BankingGatewayComponentSurface(IObjectBuilder builder)
            : base(builder)
        {
            Name = "Banking Gateway Component Surface";
            IsAvailable = true;
        }

        public override void BuildReceivePorts()
        {

        }

        public override void BuildCollaborations()
        {
            // add the components for orchestrating the 
            // processing of components related to the 
            // banking aspect of the loan broker:

            var banks = this.ObjectBuilder.ResolveAll<IBank>();

            // register the component to send the information to all of the banks once the credit bureau has been created:
            AddComponent("bank.connection.manager",
                         new IntegrationComponentConfiguration()
                             {
                                 InputChannel = CreditBureau.CreditBureauComponentSurface.CREDIT_BUREAU_OUTPUT_CHANNEL,
                             });

            // register each bank to listen on their dedicated channel and send their response to the quote accumulator:
            foreach (var bank in banks)
                AddComponent(bank.GetType(), bank.InputChannel, BANK_QUOTE_ACCUMULATOR_INPUT_CHANNEL);

            // register the component to accumulate all of the replies for the banks:
            AddComponent<BankQuoteMessageAccumulator>(BANK_QUOTE_ACCUMULATOR_INPUT_CHANNEL, 
                BANK_QUOTE_AGGREGATOR_INPUT_CHANNEL);

            // register the component to accumulate all of the replies for the banks:
            AddComponent<BankQuoteMessageAggregator>(BANK_QUOTE_AGGREGATOR_INPUT_CHANNEL, 
                BANK_QUOTE_TRANSLATOR_INPUT_CHANNEL);

            // register the component to translate the final bank quote to the loan reply:
            AddComponent<BankQuoteConfirmationToLoanQuoteConfirmationTranslator>(BANK_QUOTE_TRANSLATOR_INPUT_CHANNEL, 
                LoanAcceptance.LoanBrokerComponentSurface.LOAN_QUOTE_OUTPUT_CHANNEL);

        }

        public override void BuildSendPorts()
        {

        }

        public override void BuildErrorPort()
        {

        }
    }
}