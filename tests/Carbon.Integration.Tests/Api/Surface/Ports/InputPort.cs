using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Factory;

namespace Carbon.Integration.Tests.Api.Surface.Ports
{
    /// <summary>
    /// Port holding configurations for all input channels (i.e. receiving messages from a physical location).
    /// </summary>
    public class InputPort : AbstractPort
    {
        /// <summary>
        /// (Read-Write). Port that will hold the configuration to poll 
        /// a given physical location for a message and load it onto 
        /// a channel for processing.
        /// </summary>
        public AbstractInputChannelAdapter Port { get; private set; }

        public InputPort(string channel, string uri)
        {
            this.CreatePort(channel, uri);
        }

        public InputPort(string channel, string uri, int concurrency, int frequency)
        {
            this.CreatePort(channel, uri, concurrency, frequency);
        }

        public InputPort(string channel, string uri, int scheduled)
        {
            this.CreatePort(channel, uri, scheduled);
        }

        public override void Build()
        {
            var factory = Kernel.Resolve<IAdapterFactory>();
            var adapter = factory.BuildInputAdapterFromUri(Channel, Uri);

            if(this.Schedule > 0)
            {
                adapter.Interval = Schedule;
            }
            else
            {
                adapter.Concurrency = Concurrency;
                adapter.Frequency = Frequency;
            }

            Port = adapter;
        }
    }
}