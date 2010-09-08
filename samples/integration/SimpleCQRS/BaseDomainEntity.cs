using System;
using System.Collections.Generic;
using SimpleCQRS.Events;

namespace SimpleCQRS
{
    public abstract class BaseDomainEntity : IDomainEntity
    {
        private IDictionary<Type, Action<IDomainEvent>> _registered_events = null;

        private List<IDomainEvent> _changes = new List<IDomainEvent>();

        private IAggregateRoot _parent;

        private Guid _id;

        private int _version;

        public IAggregateRoot Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public int Version
        {
            get { return _version; }
            set { _version = value; }
        }

        public void ApplyChange(IDomainEvent domainEvent)
        {
            ApplyChange(domainEvent, true);
        }

        public void RegisterEvent<TEvent>(Action<IDomainEvent> handler) where TEvent : class, IDomainEvent
        {
            var kvp = new KeyValuePair<Type, Action<IDomainEvent>>(typeof(TEvent), handler);
            _registered_events.Add(kvp);
        }

        public abstract void RegisterEvents();

        private void ApplyChange(IDomainEvent domainEvent, bool isNew)
        {
            Action<IDomainEvent> handle;

            if (!_registered_events.TryGetValue(domainEvent.GetType(), out handle))
                throw new Exception(string.Format("There was not a handler present on '{0}' to handle the domain event '{1}'.",
                                                  this.GetType().FullName, domainEvent.GetType().FullName));

            domainEvent.AggregateRootId = this.Parent.Id;
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
}