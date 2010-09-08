using System.Collections.Generic;

namespace Carbon.Core.Channel.Impl.Queue
{
    /// <summary>
    /// Internal storage for messages that stored by location name for the channel.
    /// </summary>
    public class QueueChannelStorageLocation
    {
        public string Location { get; private set; }

        public Queue<IEnvelope> Messages { get; set; }

        public QueueChannelStorageLocation(string location)
        {
            this.Location = location;
            this.Messages = new Queue<IEnvelope>();
        }

        public void StoreMessage(IEnvelope message)
        {
            
        }
    }
}