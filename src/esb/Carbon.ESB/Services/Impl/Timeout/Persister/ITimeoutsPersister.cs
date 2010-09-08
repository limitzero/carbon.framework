using Carbon.ESB.Messages;

namespace Carbon.ESB.Services.Impl.Timeout.Persister
{
    /// <summary>
    /// Contract for all timeouts that can be persisted to a datastore.
    /// </summary>
    public interface ITimeoutsPersister
    {
        /// <summary>
        /// This will find the set of messages in the persistance store that have exceeded 
        /// their duration for delay.
        /// </summary>
        /// <returns></returns>
        TimeoutMessage[] FindAllExpiredTimeouts();

        /// <summary>
        /// This will take a current cancellation message and search 
        /// for all timeout messages associated with the message to 
        /// delay for delivery and remove the requests.
        /// </summary>
        /// <param name="message"></param>
        void AbortTimeout(CancelTimeoutMessage message);

        /// <summary>
        /// This will register the timeout in the persistance store.
        /// </summary>
        /// <param name="message"></param>
        void Save(TimeoutMessage message);

        /// <summary>
        /// This will remove the timeout from the persistance store.
        /// </summary>
        /// <param name="message"></param>
        void Complete(TimeoutMessage message);
    }
}