using System;

namespace SimpleCQRS.Events
{
    public interface IDomainEvent
    {
        Guid AggregateRootId { get; set; }
        int Version { get; set; }    
    }

    public class DomainEvent : IDomainEvent
    {
        public Guid AggregateRootId { get; set; }
        public int Version { get; set; }    
    }
}