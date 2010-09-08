using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Pipeline.Receive;

namespace Carbon.Integration.Dsl.Surface.Ports
{
    /// <summary>
    /// Port holding configurations for all input channels (i.e. receiving messages from a physical location).
    /// </summary>
    public class InputPort : AbstractPort
    {
        private readonly AbstractReceivePipeline m_pipeline;

        /// <summary>
        /// (Read-Only). Custom pipeline for processing the message post-receive.
        /// </summary>
        public AbstractReceivePipeline Pipeline { get; private set; }

        /// <summary>
        /// (Read-Write). Port that will hold the configuration to poll 
        /// a given physical location for a message and load it onto 
        /// a channel for processing.
        /// </summary>
        public AbstractInputChannelAdapter Port { get; private set; }

        public InputPort(AbstractReceivePipeline pipeline, string channel, string uri)
        {
            Pipeline = pipeline;
            this.CreatePort(channel, uri);
        }

        public InputPort(AbstractReceivePipeline pipeline, string channel, string uri, int concurrency, int frequency)
        {
            Pipeline = pipeline;
            this.CreatePort(channel, uri, concurrency, frequency);
        }

        public InputPort(AbstractReceivePipeline pipeline, string channel, string uri, int scheduled)
        {
            Pipeline = pipeline;
            this.CreatePort(channel, uri, scheduled);
        }

        public override void Build()
        {
            var factory = ObjectBuilder.Resolve<IAdapterFactory>();
            var adapter = factory.BuildInputAdapterFromUri(Channel, Uri);

            if(this.Schedule > 0)
            {
                adapter.Interval = Schedule;
            }
            else
            {
                adapter.Concurrency = Concurrency == 0 ? 1 : Concurrency;
                adapter.Frequency = Frequency == 0 ? 1 : Frequency;
            }

            // set the receive pipeline (if needed):
            if (Pipeline != null)
                adapter.Pipeline = Pipeline;

            Port = adapter;
        }
    }
}