using System;
using System.Collections.Generic;

namespace Carbon.Core.Channel.Impl.Queue
{
    /// <summary>
    /// Implementation of an "in-memory" channel that transmits and receives messages from virtual storage.
    /// </summary>
    public class QueueChannel : AbstractChannel
    {
        private QueueChannelStorageRepository _repository = null;

        /// <summary>
        /// Initializes a new instance of an in-memory channel by name.
        /// </summary>
        /// <param name="name">Unique name of the channel for transmitting and receiving messages</param>
        public QueueChannel(string name)
        {
            base.SetChannelName(name);
            _repository = new QueueChannelStorageRepository();
            _repository.AddQueueChannel(new QueueChannelStorageLocation(this.Name));
        }

        /// <summary>
        /// This will return the number of messages that are currently present 
        /// in the internal storage area.
        /// </summary>
        /// <returns></returns>
        public ICollection<IEnvelope> GetMessages()
        {
            if (_repository == null) return new List<IEnvelope>();

            var storage = _repository.FindByLocation(this.Name);

            if (storage == null) return new List<IEnvelope>();

            var list = new List<IEnvelope>(storage.Messages);
            return list.AsReadOnly();
        }

        public override IEnvelope DoReceive()
        {
            if (_repository == null) return new NullEnvelope();

            var envelope = _repository.RetreiveMessageFromLocation(this.Name);

            return envelope;
        }

        public override void DoSend(IEnvelope envelope)
        {
            if (_repository == null) return;

            if (!base.IsIdempotent)
            {
                var storage = _repository.FindByLocation(this.Name);
                if (!storage.Messages.Contains(envelope))
                    _repository.SendMessageToLocation(this.Name, envelope);
            }
            else
            {
                _repository.SendMessageToLocation(this.Name, envelope);
            }

        }

        ~QueueChannel()
        {
            _repository = null;
        }

    }
}