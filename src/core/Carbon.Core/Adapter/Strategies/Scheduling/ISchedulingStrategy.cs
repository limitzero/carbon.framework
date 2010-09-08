
namespace Carbon.Core.Adapter.Strategies.Scheduling
{
    /// <summary>
    /// Contract for periodically polling a resource for messages.
    /// </summary>
    public interface ISchedulingStrategy
    {
        /// <summary>
        /// (Read-Write). The amount of time, in seconds, that the item should be polled.
        /// </summary>
        int Interval { get; set; }
    }
}