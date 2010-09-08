using Carbon.Core.Adapter;
using Carbon.Core.Adapter.Factory;

namespace Carbon.Integration.Tests.Api.Surface.Ports
{
    /// <summary>
    /// Port holding configurations for all output channels (i.e. sending messages to a location).
    /// </summary>
    public class OutputPort : AbstractPort
    {
        /// <summary>
        /// (Read-Only). The port that will contain the configuration 
        /// for taking a message from a channel and loading it to a 
        /// physical location for storage.
        /// </summary>
        public AbstractOutputChannelAdapter Port { get; private set; }

        public OutputPort(string channel, string uri)
        {
            this.CreatePort(channel, uri);
        }

        public override void Build()
        {
            var factory = Kernel.Resolve<IAdapterFactory>();
            var adapter = factory.BuildOutputAdapterFromUri(Channel, Uri);

            if (this.Schedule > 0)
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