using Carbon.ESB.Saga;

namespace Domain
{
    /// <summary>
    /// Base class representing a logical context for separation of 
    /// logical processing within the domain.
    /// </summary>
    public class AggregateRoot : Saga, IAggregateRoot
    {
        /// <summary>
        /// (Read-Write). The current version of the aggregate root.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// (Read-Write). The name of the aggregate root (typically the domain object).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// (Read-Write). The current event that caused a change in the aggregate root.
        /// </summary>
        public string Event { get; set; }
        
        /// <summary>
        /// This will record the event that caused the change 
        /// on the aggregate root and store the corresponding
        /// information.
        /// </summary>
        /// <param name="message">Event message that caused the change in the aggregate root.</param>
        public void ApplyEvent(object message)
        {
            // record the event that caused the change, 
            // the aggregate root object type, 
            // and increment the version number of the 
            // aggregate root:
            Name = this.GetType().FullName;
            Event = message.GetType().FullName;
            Version++;
        }
    }
}
