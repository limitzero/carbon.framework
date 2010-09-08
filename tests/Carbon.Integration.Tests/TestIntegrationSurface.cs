using Carbon.Core.Builder;
using Carbon.Core.Pipeline.Component;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send;
using Carbon.Integration.Dsl.Surface;
using Carbon.Integration.Dsl.Surface.Ports;

namespace Carbon.Integration.Tests
{
    public class TestIntegrationSurface 
        : AbstractIntegrationComponentSurface
    {
   
        public TestIntegrationSurface(IObjectBuilder builder) : base(builder)
        {
            Name = "Test Integration Surface";
            IsAvailable = true;
        }

        public override void BuildReceivePorts()
        {
            // create ports for accepting messages:
            CreateReceivePort(this.GetFileReceivePipeline(), "in", @"file://c:\trash\incoming", 4, 1);   
        }

        public override void BuildSendPorts()
        {
            // create ports for sending messages:
            CreateSendPort(this.GetFileSendPipeline(), "out", @"file://c:\trash\outgoing", 1, 1);
        }

        public override void BuildErrorPort()
        {
            // create an error handling port where all messages will be sent:
            var config = new ErrorOutputPortConfiguration("test_integration_error", @"file://c:\trash\errors");

            //CreateErrorPort(null, "test_integration_error", @"file://c:\trash\outgoing");
            CreateErrorPort(null, config);
        }

        public override void BuildCollaborations()
        {
            // add the components that will participate in the integration:
            //AddComponent<TestComponent>("in", "out", string.Empty);
            AddComponent<TestComponent>();  
            // AddComponent<PassThroughComponentFor<string>>("in","out");
        }

        private AbstractReceivePipeline GetFileReceivePipeline()
        {
            var pipeline = new ReceivePipeline() {Name = "File Mover Receive Pipeline"};

            // change the byte[] representation of the message to a string
            // and make sure that no duplicate messages are received...
            pipeline.RegisterComponents(
                ObjectBuilder.Resolve<BytesToStringPipelineComponent>(),
                ObjectBuilder.Resolve<NonIdempotentPipelineComponent>()
                );

            return pipeline;
        }

        private AbstractSendPipeline GetFileSendPipeline()
        {
            var pipeline = new SendPipeline() {Name = "File Mover Send Pipeline"};
            pipeline.RegisterComponents(
                ObjectBuilder.Resolve<AppendDateTimeToMessagePipelineComponent>());
                //ObjectBuilder.Resolve<GenerateExceptionPipelineComponent>());
            return pipeline;
        }

    }

}