using System;

namespace Carbon.Core.Adapter
{
    /// <summary>
    /// Contract for an adapter.
    /// </summary>
    public interface IAdapter
    {
        /// <summary>
        /// Event that is triggered when the channel adapter is started.
        /// </summary>
        event EventHandler<ChannelAdapterStartedEventArgs> AdapterStarted;

        /// <summary>
        /// Event that is triggered when the channel adapter is started.
        /// </summary>
        event EventHandler<ChannelAdapterStoppedEventArgs> AdapterStopped;

        /// <summary>
        /// Event that is triggered when the channel adapter encounters an error.
        /// </summary>
        event EventHandler<ChannelAdapterErrorEventArgs> AdapterError;

        /// <summary>
        /// (Read-Only). The channel where the message will either be unloaded to a storage location or uploaded from a storage location.
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// (Read-Write). Flag to indicate whether the adapter supports transactions.
        /// </summary>
        bool IsTransactional { get; set; }

        /// <summary>
        /// (Read-Write). The uri configuration used by the adapter to access the physical location where messages will be stored or retrieved.
        /// </summary>
        string Uri { get; set; }

        /// <summary>
        /// This will set the channel where the message will either be unloaded to a storage location or uploaded from a storage location.
        /// </summary>
        /// <param name="channelName"></param>
        void SetChannel(string channelName);

        /// <summary>
        /// This will set the registry internally where all of the channels are defined.
        /// </summary>
        /// <param name="registry"></param>
        //void SetChannelRegistry(IChannelRegistry registry);

        /// <summary>
        /// This will return the scheme used by the adapter.
        /// </summary>
        string GetScheme();
    }
}