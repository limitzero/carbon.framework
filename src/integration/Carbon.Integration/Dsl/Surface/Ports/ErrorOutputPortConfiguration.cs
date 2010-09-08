namespace Carbon.Integration.Dsl.Surface.Ports
{
    public class ErrorOutputPortConfiguration : OutputPortConfiguration
    {
        public int MaxRetries { get; set; }
        public int WaitInterval { get; set; }

         public ErrorOutputPortConfiguration(string channel, string uri)
             : base(channel, uri)
        {

        }

        public ErrorOutputPortConfiguration(string channel, string uri, int schedule)
            : base(channel, uri, schedule)
        {
        }

        public ErrorOutputPortConfiguration(string channel, string uri, int concurrency, int frequency)
            :base(channel, uri, concurrency, frequency)
        {

        }
    }
}