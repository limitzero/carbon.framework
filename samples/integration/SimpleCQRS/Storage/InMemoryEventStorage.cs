using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Events;

namespace SimpleCQRS.Storage
{
    public class InMemoryEventStorage : IEventStorage
    {
        private static IDictionary<Guid, IList<EventStorageItem>> _storage = null;

        public InMemoryEventStorage()
        {
            if(_storage == null)
                _storage = new   Dictionary<Guid, IList<EventStorageItem>>();
        }

        public IEnumerable<IDomainEvent> GetEvents(Guid id)
        {
            IList<EventStorageItem> items;

            if(!_storage.TryGetValue(id, out items))
                throw new Exception("No events could be produced from storage for aggregate id " + id.ToString());

            return (IEnumerable<IDomainEvent>) items.ToArray();
        }

        public void StoreEvents(Guid id, List<IDomainEvent> changes, int version)
        {
            var events = (from de in changes
                                 select new EventStorageItem()
                                    {
                                        Created = DateTime.Now,
                                        Event = de,
                                        Id = id,
                                        Version = version
                                    }).ToList();

            if(!_storage.ContainsKey(id))
                _storage.Add(id, events);

        }
    }

    [Serializable]
    public class EventStorageItem
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public IDomainEvent Event { get; set; }
        public DateTime Created { get; set; }
    }
}