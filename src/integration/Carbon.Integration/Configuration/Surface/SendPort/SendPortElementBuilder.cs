using System;
using Carbon.Core.Configuration;
using Carbon.Core.Pipeline.Send;
using Carbon.Integration.Configuration.Surface.Pipeline;
using Carbon.Integration.Dsl.Surface.Ports;
using Castle.Core.Configuration;

namespace Carbon.Integration.Configuration.Surface.SendPort
{
    public class SendPortElementBuilder : AbstractSubElementBuilder
    {
        private const string m_element_name = "send-port";

        public OutputPort Port { get; private set; }

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name.Trim().ToLower();
        }

        public override void Build(IConfiguration configuration)
        {
            var channel = configuration.Attributes["channel"];
            var uri = configuration.Attributes["uri"];
            var concurrency = configuration.Attributes["concurrency"];
            var frequency = configuration.Attributes["frequency"];
            var scheduled = configuration.Attributes["scheduled"];

            // build the pipeline for the send port:
            var pipelineBuilder = new PipelineBuilder(this.Kernel, configuration);
            var pipeline = pipelineBuilder.BuildSendPipeline();

            Port = this.CreatePort(pipeline, channel, uri, concurrency, frequency, scheduled);
        }

        private OutputPort CreatePort(AbstractSendPipeline pipeline,
                                      string channel, string uri,
                                      string concurrency, string frequency, string scheduled)
        {
            OutputPort port = null;
            var value = 0;

            if (!string.IsNullOrEmpty(scheduled))
            {
                if (Int32.TryParse(scheduled, out value))
                    port = new OutputPort(pipeline, channel, uri, value);
            }
            else
            {
                var parsedConcurrency = 1;
                var parsedFrequency = 1;

                if (!string.IsNullOrEmpty(concurrency))
                    if (Int32.TryParse(concurrency, out parsedConcurrency)) ;

                if (!string.IsNullOrEmpty(frequency))
                    if (Int32.TryParse(frequency, out parsedFrequency)) ;

                port = new OutputPort(pipeline, channel, uri, parsedConcurrency, parsedFrequency);
            }

            return port;
        }
    }
}