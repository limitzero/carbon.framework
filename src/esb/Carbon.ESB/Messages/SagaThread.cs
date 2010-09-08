using System;
using System.Xml.Serialization;

namespace Carbon.ESB.Messages
{
    /// <summary>
    /// Class that holds the current information for a long-running conversation that can be persisted offline.
    /// </summary>
    [Serializable]
    public class SagaThread : ISagaThread
    {
        /// <summary>
        /// (Read-Write). The unqiue identifier for the current conversation.
        /// </summary>
        public Guid SagaId { get; set; }

        /// <summary>
        /// (Read-Write). The name of the class plus assembly that is currently participating in a converation.
        /// </summary>
        public string SagaName { get; set; }

        /// <summary>
        /// (Read-Write). The serialized version of the conversation (in byte form).
        /// </summary>
        public byte[] Saga { get; set; }

        /// <summary>
        /// (Read-Write). The instance of the conversation.
        /// </summary>
        [XmlIgnore]
        public object SagaInstance { get; set; }

        public DateTime? CreateDateTime { get; set;}

    }
}