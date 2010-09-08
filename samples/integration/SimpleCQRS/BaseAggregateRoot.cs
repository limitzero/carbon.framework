using System;
using System.Collections.Generic;
using SimpleCQRS.Events;

namespace SimpleCQRS
{
    public abstract class BaseAggregateRoot<TEvent> : 
        IAggregateRoot<TEvent> where TEvent : IDomainEvent
    {
        private IDictionary<Type, Action<TEvent>> _registered_events = null;

        private List<TEvent> _changes = new List<TEvent>();

        public Guid Id { get; set; }
        public int Version { get; set; }

        protected BaseAggregateRoot()
        {
            Id = Guid.NewGuid();
            _registered_events = new Dictionary<Type, Action<TEvent>>();
        }

        public List<TEvent> GetUnCommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsUnCommitted()
        {
            _changes.Clear();
        }

        public void LoadFromHistory(IEnumerable<TEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
                this.ApplyChange(domainEvent, false);
        }

        public void ApplyChange(TEvent domainEvent)
        {
            ApplyChange(domainEvent, true);
        }

        public void RegisterEvent<TEvent>(Action<TEvent> handler) where TEvent : class, IDomainEvent 
        {
            _registered_events.Add(typeof(TEvent), theEvent => handler(theEvent as TEvent));
        }

        public abstract void RegisterEvents(); 

        private void ApplyChange(TEvent domainEvent, bool isNew)
        {
            Action<TEvent> handle;

            if(!_registered_events.TryGetValue(domainEvent.GetType(), out handle))
                throw new Exception(string.Format("There was not a handler present on '{0}' to handle the domain event '{1}'.", 
                                                  this.GetType().FullName, domainEvent.GetType().FullName));

            domainEvent.AggregateRootId = this.Id;
            domainEvent.Version = GetCurrentVersion();

            if (isNew)
                _changes.Add(domainEvent);

            handle(domainEvent);
        }

        private int GetCurrentVersion()
        {
            return Version++;
        }

    }

    public abstract class BaseEntity
    {
        
    }
}