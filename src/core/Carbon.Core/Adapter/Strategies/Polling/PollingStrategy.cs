namespace Carbon.Core.Adapter.Strategies.Polling
{
    /// <summary>
    /// Default strategy for polling a resource.
    /// </summary>
    public class PollingStrategy : IPollingStrategy
    {
        public int Frequency { get; set; }
        public int Concurrency { get; set; }

        public PollingStrategy()
            :this(1, 1)
        {
        }

        public PollingStrategy(int concurrency, int frequency)
        {
            Frequency = frequency;
            Concurrency = concurrency;
        }
    }
}