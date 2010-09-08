namespace Carbon.Core.Adapter.Strategies.Polling
{
    /// <summary>
    /// Strategy for polling a resource.
    /// </summary>
    public interface IPollingStrategy
    {
        /// <summary>
        /// (Read-Write). The amount of time to wait, in seconds, before polling the resource.
        /// </summary>
        int Frequency { get; set; }

        /// <summary>
        /// (Read-Write). The number of active threads used to process the information at the resource.
        /// </summary>
        int Concurrency { get; set; }
    }
}