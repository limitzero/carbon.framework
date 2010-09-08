using System;
using System.Collections.Generic;
using SimpleCQRS.Events;

namespace SimpleCQRS.Storage
{
    /// <summary>
    /// Contract for pushing all domain events to a storage location.
    /// </summary>
    public interface IEventStorage
    {
        IEnumerable<IDomainEvent> GetEvents(Guid guid);
        void StoreEvents(Guid guid, List<IDomainEvent> changes, int version);
    }
}