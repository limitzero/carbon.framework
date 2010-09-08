using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Integration.Dsl.Surface.Ports
{
    public class OutputPortConfiguration : IPortDefinition
    {
        public string Channel { get; private set; }
        public string Uri { get; private set; }
        public int Concurrency { get; private set; }
        public int Frequency { get; private set; }
        public int Schedule { get; private set; }

        public OutputPortConfiguration(string channel, string uri)
        {
            this.Channel = channel;
            this.Uri = uri;
        }

        public OutputPortConfiguration(string channel, string uri, int schedule)
        {
            this.Channel = channel;
            this.Uri = uri;
            this.Schedule = schedule;
        }

        public OutputPortConfiguration(string channel, string uri, int concurrency, int frequency)
        {
            this.Channel = channel;
            this.Uri = uri;
            this.Concurrency = concurrency;
            this.Frequency = frequency;
        }
    }
}
