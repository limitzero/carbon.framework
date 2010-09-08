using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Strategies.Retry;
using Carbon.Core.Pipeline.Send;

namespace Carbon.Integration.Dsl.Surface.Ports
{
    /// <summary>
    /// Port holding configurations for all output channels (i.e. sending messages to a location).
    /// </summary>
    public class OutputPort : AbstractPort
    {
        private readonly AbstractSendPipeline m_pipeline;
        private readonly OutputPortConfiguration m_configuration;

        /// <summary>
        /// (Read-Only). Custom pipeline for processing the message pre-send.
        /// </summary>
        public AbstractSendPipeline Pipeline { get; private set; }

        /// <summary>
        /// (Read-Only). The port that will contain the configuration 
        /// for taking a message from a channel and loading it to a 
        /// physical location for storage.
        /// </summary>
        public AbstractOutputChannelAdapter Port { get; private set; }

        public OutputPort(AbstractSendPipeline pipeline, OutputPortConfiguration configuration)
        {
            Pipeline = pipeline;
            m_configuration = configuration;
            this.BuildPortFromConfiguration();
        }

        public OutputPort(AbstractSendPipeline pipeline, string channel, string uri)
        {
            Pipeline = pipeline;
            this.CreatePort(channel, uri);
        }

        public OutputPort(AbstractSendPipeline pipeline, string channel, string uri, int concurrency, int frequency)
        {
            Pipeline = pipeline;
            this.CreatePort(channel, uri, concurrency, frequency);
        }

        public OutputPort(AbstractSendPipeline pipeline ,string channel, string uri, int scheduled)
        {
            Pipeline = pipeline;
            this.CreatePort(channel, uri, scheduled);
        }

        public override void Build()
        {
            var factory = ObjectBuilder.Resolve<IAdapterFactory>();
            var adapter = factory.BuildOutputAdapterFromUri(Channel, Uri);

            if (this.Schedule > 0)
            {
                adapter.Interval = Schedule;
            }
            else
            {
                adapter.Concurrency = Concurrency == 0 ? 1 : Concurrency;
                adapter.Frequency = Frequency == 0 ? 1 :Frequency;
            }

            if (Pipeline != null)
                adapter.Pipeline = Pipeline;

            if (m_configuration != null)
            {
                if (typeof (ErrorOutputPortConfiguration).IsAssignableFrom(m_configuration.GetType()))
                {
                    var cfg = m_configuration as ErrorOutputPortConfiguration;

                    if (cfg.MaxRetries > 0)
                    {
                        adapter.RetryStrategy = new RetryStrategy(cfg.MaxRetries, cfg.WaitInterval);
                    }
                }
            }

            Port = adapter;
        }

        private void BuildPortFromConfiguration()
        {
            if (m_configuration.Concurrency > 0)
            {
                this.CreatePort(m_configuration.Channel, m_configuration.Uri, m_configuration.Concurrency,
                                m_configuration.Frequency);
                return;
            }

            if (m_configuration.Schedule > 0)
            {
                this.CreatePort(m_configuration.Channel, m_configuration.Uri, m_configuration.Concurrency,
                                m_configuration.Schedule);
                return;
            }

            if(m_configuration.Concurrency == 0 && m_configuration.Schedule == 0)
            {
                this.CreatePort(m_configuration.Channel, m_configuration.Uri);
                return;
            }

        }
    }
}