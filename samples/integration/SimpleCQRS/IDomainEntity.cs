using System;
using SimpleCQRS.Events;

namespace SimpleCQRS
{
    public interface IDomainEntity
    {
        IAggregateRoot Parent { get; set; }
        Guid Id { get; set; }
        int Version { get; set; }
        void ApplyChange(IDomainEvent domainEvent);
    }
}