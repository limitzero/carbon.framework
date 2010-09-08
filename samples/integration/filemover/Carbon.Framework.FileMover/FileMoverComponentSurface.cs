using Carbon.Core.Builder;
using Carbon.Core.Components;
using Carbon.Core.Pipeline.Component;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send;
using Carbon.Integration.Dsl.Surface;

namespace Carbon.Framework.FileMover
{
    public class FileMoverComponentSurface : AbstractIntegrationComponentSurface
    {
        // using the file uri notation for inspecting directories
        // you can set the source and target directories to the full
        // file uri notation from the config file if needed:
        public string InputChannel { get; set; }
        public string OutputChannel { get; set; }
        public string SourceLocation { get; set; }
        public string TargetLocation { get; set; }

        public FileMoverComponentSurface(IObjectBuilder builder)
            : base(builder)
        {
            Name = "Five Mover Component Surface";
            IsAvailable = true;
        }

        public override void BuildReceivePorts()
        {
            // build the receive port to direct the message to the 
            // desired channel for processing, optionally including 
            // a pipeline for altering the message post-receive:

            // for this instance, create a receive port that will
            // poll the receive location with 1 worker threads
            // polling at 1 second intervals, and take any message
            // and load it onto the channel called "in":
            CreateReceivePort(this.GetReceivePipeline(), InputChannel, SourceLocation, 1, 1);
        }

        public override void BuildCollaborations()
        {
            // here is where we can introduce our own 
            // custom logic for handling the contents 
            // of the received messages:

            // let's use the pass-through component for this 
            // interaction as we will not change the contents 
            // of the message (we will need to process the string data
            // that comes back from the receive location), also we will expect 
            // the message to be present on the channel called "in" and 
            // after processing we will send it to a channel called "out":
            AddComponent<PassThroughComponentFor<string>>(InputChannel, OutputChannel);
        }

        public override void BuildSendPorts()
        {
            // build the send port to direct the message to the target location
            // optionally include a pipeline for altering the message pre-send:

            // for this instance, create a send port that will inspect 
            // the channel called "out" for any messages with 1 worker 
            // threads polling at 1 second, and send the messages to the 
            // target location:
            CreateSendPort(this.GetSendPipeline(), OutputChannel, TargetLocation, 1, 1);
        }

        public override void BuildErrorPort()
        {
            // can create custom error port where any messages
            // will go if the processing for this surface encounters 
            // an error:
        }

        private AbstractReceivePipeline GetReceivePipeline()
        {
            // since our file adapter will return the byte[]
            // representation of the message, let's change it to 
            // a string form before sending to the component(s)
            // for processing:
            var pipeline = new ReceivePipeline() { Name = "File Receive Pipeline" };

            // add the pre-fabricated component to change bytes to a string representation:
            pipeline.RegisterComponents(ObjectBuilder.Resolve<BytesToStringPipelineComponent>());

            return pipeline;
        }

        private AbstractSendPipeline GetSendPipeline()
        {
            // since we are delivering string message to the file location,
            // we don't really need a pipeline to handle this as the file adapter
            // will translate a string into byte[] format for writing, but let's define
            // one anyway for illustrative purposes:

            var pipeline = new SendPipeline() {Name = "File Send Pipeline"};
            pipeline.RegisterComponents(ObjectBuilder.Resolve<StringToBytesPipelineComponent>());

            return pipeline;
        }

    }
}