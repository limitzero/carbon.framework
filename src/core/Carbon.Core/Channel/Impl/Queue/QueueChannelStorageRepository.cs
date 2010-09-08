using System.Collections.Generic;
using System.Diagnostics;

namespace Carbon.Core.Channel.Impl.Queue
{
    /// <summary>
    /// Internal repository for all storage locations.
    /// </summary>
    public class QueueChannelStorageRepository
    {
        private List<QueueChannelStorageLocation> m_queue_channel_locations;
        private object m_add_queue_location_lock = new object();

        public QueueChannelStorageRepository()
        {
            if (m_queue_channel_locations == null)
                m_queue_channel_locations = new List<QueueChannelStorageLocation>();
        }

        public QueueChannelStorageLocation[] QueuedChannels
        {
            get { return m_queue_channel_locations.ToArray(); }
            set { m_queue_channel_locations.AddRange(value); }
        }

        public QueueChannelStorageLocation FindByLocation(string name)
        {
            QueueChannelStorageLocation channelLocation = null;

            foreach (var location in m_queue_channel_locations)
            {
                if (location.Location.Trim().ToLower() == name.Trim().ToLower())
                {
                    channelLocation = location;
                    break;
                }
            }

            return channelLocation;
        }

        public  void AddQueueChannel(QueueChannelStorageLocation location)
        {
            lock (m_add_queue_location_lock)
            {
                if (!m_queue_channel_locations.Contains(location))
                    m_queue_channel_locations.Add(location);
            }
        }

        public  void SendMessageToLocation(string location, IEnvelope message)
        {
            var storage_location = FindByLocation(location);

            if(storage_location == null) return;

            storage_location.Messages.Enqueue(message);
        }

        public IEnvelope RetreiveMessageFromLocation(string location)
        {
            IEnvelope message = new NullEnvelope();
            var needToHandle = false;
            QueueChannelStorageLocation storageLocation = null;

            try
            {
                storageLocation = FindByLocation(location);

                if (storageLocation == null)
                    return message;

                needToHandle = storageLocation.Messages.Peek() != null;
            }
            catch 
            {
                needToHandle = false;
            }

            if(needToHandle)
            {
                lock (storageLocation)
                {
                    message = storageLocation.Messages.Dequeue();
                }
            }

            return message;
        }

        ~QueueChannelStorageRepository()
        {
            if(m_queue_channel_locations == null)
                return;

            m_queue_channel_locations.Clear();
            m_queue_channel_locations = null;
        }
    }
}