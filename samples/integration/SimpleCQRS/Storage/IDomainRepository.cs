using System;

namespace SimpleCQRS.Storage
{
    /// <summary>
    ///  Contract for storing all domain events.
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    public interface IDomainRepository<TAggregateRoot> where TAggregateRoot : IAggregateRoot, new()
    {
        TAggregateRoot Find(Guid id);
        void Store(TAggregateRoot aggregateRoot);
    }

    public class DomainRepository<TAggregateRoot> : IDomainRepository<TAggregateRoot> where TAggregateRoot : IAggregateRoot, new()
    {
        private readonly IEventStorage _storage;

        public DomainRepository(IEventStorage storage)
        {
            _storage = storage;
        }

        public TAggregateRoot Find(Guid id)
        {
            var aggregate = new TAggregateRoot();
            var events = _storage.GetEvents(id);
            aggregate.LoadFromHistory(events);

            return aggregate;
        }

        public void Store(TAggregateRoot aggregateRoot)
        {
            _storage.StoreEvents(aggregateRoot.Id, aggregateRoot.GetUnCommittedChanges(), aggregateRoot.Version);
        }
    }
}