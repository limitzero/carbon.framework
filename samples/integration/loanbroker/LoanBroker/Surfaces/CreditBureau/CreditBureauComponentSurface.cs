using System;
using Carbon.Core.Builder;
using Carbon.Integration.Dsl.Surface;
using LoanBroker.Surfaces.CreditBureau.Components;

namespace LoanBroker.Surfaces.CreditBureau
{
    public class CreditBureauComponentSurface
        : AbstractIntegrationComponentSurface
    {
        public const string CREDIT_BUREAU_INPUT_CHANNEL = "credit.bureau.request";
        public const string CREDIT_BUREAU_OUTPUT_CHANNEL = "credit.bureau.replies";

        public CreditBureauComponentSurface(IObjectBuilder builder) : base(builder)
        {
            Name = "Credit Bureau Component Surface";
            IsAvailable = true;
        }

        public override void BuildReceivePorts()
        {
            
        }

        public override void BuildCollaborations()
        {
            AddComponent<CreditBureauMessageConsumer>(CREDIT_BUREAU_INPUT_CHANNEL, 
                CREDIT_BUREAU_OUTPUT_CHANNEL);
        }

        public override void BuildSendPorts()
        {

        }

        public override void BuildErrorPort()
        {

        }
    }
}