using System;
using System.Xml.Serialization;

namespace Carbon.ESB.Saga
{
    /// <summary>
    /// Basic contract that will allow stateful message interchange over a channel 
    /// for long-running processes.
    /// </summary>
    public interface ISaga
    {
        /// <summary>
        /// (Read-Write). Unique identifier of the saga instance.
        /// </summary>
        Guid SagaId { get; set; }

        /// <summary>
        /// (Read-Write). Flag to denote when the saga has been completed.
        /// </summary>
        bool IsCompleted { get; set; }
    }

    public interface IMessagingSaga
    {
        /// <summary>
        /// (Read-Write). The current message bus instance used for mediating the 
        /// messages used in the conversation between endpoints.
        /// </summary>
        //[XmlIgnore]
        //IMessageBus Bus { get; set; }

        void SetMessageBus(IMessageBus bus);

        void Publish(params ISagaMessage[] messages);

        void Publish(params object[] messages);
    }
}