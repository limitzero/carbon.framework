using System;
using System.Xml.Serialization;

namespace Carbon.ESB.Messages
{
    public interface ISagaThread
    {
        /// <summary>
        /// (Read-Write). The unqiue identifier for the current conversation.
        /// </summary>
        Guid SagaId { get; set; }

        /// <summary>
        /// (Read-Write). The name of the class plus assembly that is currently participating in a converation.
        /// </summary>
        string SagaName { get; set; }

        /// <summary>
        /// (Read-Write). The serialized version of the conversation (in byte form).
        /// </summary>
        byte[] Saga { get; set; }

        /// <summary>
        /// (Read-Write). The instance of the conversation.
        /// </summary>
        [XmlIgnore]
        object SagaInstance { get; set; }

        /// <summary>
        /// (Read-Write). Date/time the saga thread created.
        /// </summary>
        DateTime? CreateDateTime { get; set; }
    }
}