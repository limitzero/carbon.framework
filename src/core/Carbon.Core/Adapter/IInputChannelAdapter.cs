using System;

namespace Carbon.Core.Adapter
{
    /// <summary>
    /// Contract for an adapter that can take a message from a physical location and load it to a channel for processing.
    /// This adapter can be either polled or scheduled to inspect the physical location on a periodic basis.
    /// </summary>
    public interface IInputChannelAdapter : IAdapter
    {
        /// <summary>
        /// Event that is triggered when a message is picked up from the physical location and loaded into a channel.
        /// </summary>
        event EventHandler<ChannelAdapterMessageReceivedEventArgs> AdapterMessageReceived; 
    }
}