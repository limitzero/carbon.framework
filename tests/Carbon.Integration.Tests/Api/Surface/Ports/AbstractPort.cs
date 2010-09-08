using Castle.MicroKernel;

namespace Carbon.Integration.Tests.Api.Surface.Ports
{
    public abstract class AbstractPort : IPort
    {
        public IKernel Kernel { get; set; }
        public string Channel { get; private set; }
        public string Uri { get; private set; }
        public int Concurrency { get; private set; }
        public int Frequency { get; private set; }
        public int Schedule { get; private set; }

        public IPort CreatePort(string channel, string uri)
        {
            this.Channel = channel;
            this.Uri = uri;
            return this;
        }

        public IPort CreatePort(string channel, string uri, int concurrency, int frequency)
        {
            this.Channel = channel;
            this.Uri = uri;
            this.Concurrency = concurrency;
            this.Frequency = frequency;
            return this;
        }

        public IPort CreatePort(string channel, string uri, int scheduled)
        {
            this.Channel = channel;
            this.Uri = uri;
            this.Schedule = scheduled;
            return this;
        }

        public abstract void Build();
    }
}