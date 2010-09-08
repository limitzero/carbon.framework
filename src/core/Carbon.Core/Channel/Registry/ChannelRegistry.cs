using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Channel.Impl.Queue;

namespace Carbon.Channel.Registry
{
    /// <summary>
    /// Concrete instance for storing all channels that will be used for communication.
    /// </summary>
    public class ChannelRegistry : IChannelRegistry
    {
        private List<AbstractChannel> m_channels = null;

        public ReadOnlyCollection<AbstractChannel> Channels
        {
            get;
            private set;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public ChannelRegistry()
        {
            if (m_channels == null)
            {
                m_channels = new List<AbstractChannel>();
                this.Channels = m_channels.AsReadOnly();
            }
        }

        public AbstractChannel FindChannel(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new NullChannel();

            var channel = this.Channels.SingleOrDefault(item => item.Name.Trim().ToLower() == name.Trim().ToLower());

            if (channel == null)
                channel = new NullChannel();

            return channel;
        }

        public TChannel FindChannel<TChannel>(string name) where TChannel : AbstractChannel
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var channel = this.Channels.Single(item => 
                                                item.Name.Trim().ToLower() == name.Trim().ToLower() 
                                                && item.GetType() == typeof (TChannel));

            if (channel == null)
                throw new Exception(string.Format("The channel '{0}' could not be found in the registry and resolved to type '{2}'", 
                    name, typeof(TChannel).FullName));
            

            return (TChannel)channel;

        }

        public void RegisterChannel(AbstractChannel channel)
        {
            if (!(this.FindChannel(channel.Name) is NullChannel)) return;

            m_channels.Add(channel);
            this.Channels = m_channels.AsReadOnly();
        }

        public void RegisterChannel(string channel)
        {
            var queueChannel = new QueueChannel(channel);
            this.RegisterChannel(queueChannel);
        }

    }
}