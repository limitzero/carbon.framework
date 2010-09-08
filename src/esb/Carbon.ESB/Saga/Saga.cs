using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Carbon.ESB.Saga
{
    /// <summary>
    /// Contract for long-running sessions of message producing and consumption.
    /// </summary>
    [Serializable]
    public abstract class Saga : ISaga
    {
        /// <summary>
        /// (Read-Write). The unique indentifier that is assigned to the conversation.
        /// </summary>
        public Guid SagaId { get; set; }

        /// <summary>
        /// (Read-Write). Flag to check whether the conversation has been completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// (Read-Write). The current message bus instance used for mediating the 
        /// messages used in the conversation between endpoints.
        /// </summary>
        [XmlIgnore]
        public IMessageBus Bus { get; set; }

        /// <summary>
        /// Contract for recording persistant state that should be kept across 
        /// multiple conversation invocations. The state by default is auto-correlated
        /// with the conversation.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        [Serializable]
        public abstract class WithState<TState> : Saga where TState : class
        {
            /// <summary>
            /// (Read-Write). The state that is kept between multiple calls to the conversation.
            /// </summary>
            public TState State { get; set; }
        }

    }
}