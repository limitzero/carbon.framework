using System;
using System.Collections.Generic;
using SimpleCQRS.Events;
namespace SimpleCQRS
{
    public interface IAggregateRoot<TEvent> where TEvent : IDomainEvent
    {
        Guid Id { get; set; }
        int Version { get; set; }

        void RegisterEvent<TEvent>(Action<TEvent> handler) where TEvent : class, IDomainEvent;
        List<TEvent> GetUnCommittedChanges();
        void MarkChangesAsUnCommitted();
        void LoadFromHistory(IEnumerable<TEvent> domainEvents);
        void ApplyChange(TEvent domainEvent);
    }
}