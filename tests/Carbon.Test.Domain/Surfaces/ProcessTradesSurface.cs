using Carbon.Core.Builder;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send;
using Carbon.Integration.Dsl.Surface;

namespace Carbon.Test.Domain.Surfaces
{
    public class ProcessTradesSurface : AbstractIntegrationComponentSurface
    {
        public ProcessTradesSurface(IObjectBuilder builder)
            : base(builder)
        {
            Name = "Process Invoice Surface";
            IsAvailable = true;
        }

        public override void BuildReceivePorts()
        {
            var uri = "msmq://localhost/private$/invoice.inbound";
            CreateReceivePort(this.GetReceivePipeline(), uri, "trades", 2, 1);
        }

        public override void BuildCollaborations()
        {
            AddComponent<TradesConsumer>("trades","ibm_trades");
        }

        public override void BuildSendPorts()
        {
            var uri = "msmq://localhost/private$/invoice.outbound";
            CreateSendPort(this.GetSendPipeline(), uri, "ibm_trades", 2, 1);
        }

        public override void BuildErrorPort()
        {

        }

        private AbstractReceivePipeline GetReceivePipeline()
        {
            var pipeline = new DeserializeMessagePipeline(this.ObjectBuilder);
            return pipeline;
        }

        private AbstractSendPipeline GetSendPipeline()
        {
            var pipeline = new SerializeMessagePipeline(this.ObjectBuilder);
            return pipeline;
        }
    }
}