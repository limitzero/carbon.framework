using System;
using Carbon.Core.Adapter.Strategies.Retry;

namespace Carbon.Core.Adapter
{
    /// <summary>
    /// Contract for an adapter that can take a message from a channel and load it to a physical location processing.
    /// </summary>
    public interface IOutputChannelAdapter : IAdapter
    {
        /// <summary>
        /// Event that is triggered when the message is delivered to the storage location.
        /// </summary>
        event EventHandler<ChannelAdapterMessageDeliveredEventArgs> AdapterMessgeDelivered;

        /// <summary>
        /// (Read-Write). The strategy used for retrying messages upon initial submission error.
        /// </summary>
        IRetryStrategy RetryStrategy { get; set; }

        /// <summary>
        /// This will send the message to the storage location for processing. This will delegate to the <seealso cref="AbstractOutputChannelAdapter.DoSend"/>
        /// method for actual implementation.
        /// </summary>
        void Send(IEnvelope envelope);
    }
}