using System.Collections.ObjectModel;
using Carbon.Core.Channel;

namespace Carbon.Channel.Registry
{
    /// <summary>
    /// Contract for storing all channels that will be used for communication
    /// </summary>
    public interface IChannelRegistry
    {
        /// <summary>
        /// (Read-Only). The current collection of all registered channels.
        /// </summary>
        ReadOnlyCollection<AbstractChannel> Channels { get; }

        /// <summary>
        /// This will return a channel from the registry by name.
        /// </summary>
        /// <param name="name">The name of the channel</param>
        /// <returns></returns>
        AbstractChannel FindChannel(string name);

        /// <summary>
        /// This will return a channel from the registry by name and type. Note: a NullChannel value is returned 
        /// if the channel can not be resolved by name.
        /// </summary>
        /// <param name="name">The name of the channel</param>
        /// <returns>
        ///  <ul>
        ///    <li><typeparam name="TChannel">Type of the channel if found by name</typeparam></li>
        ///    <li>Null if channel can not be resolved by name</li>
        /// </ul>
        /// </returns>
        TChannel FindChannel<TChannel>(string name) where TChannel : AbstractChannel;

        /// <summary>
        /// This will register a channel.
        /// </summary>
        /// <param name="channel"></param>
        void RegisterChannel(AbstractChannel channel);

        /// <summary>
        /// This will register a <seealso cref="QueueChannel">queue channel</seealso>
        /// with the name specified.
        /// </summary>
        /// <param name="channel">The name of the channel</param>
        void RegisterChannel(string channel);

       
    }
}