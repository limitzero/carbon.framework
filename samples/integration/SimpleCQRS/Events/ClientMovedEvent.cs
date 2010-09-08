using System;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace SimpleCQRS.Events
{
    [Message]
    public class ClientMovedEvent : DomainEvent
    {
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
    }

    [Message]
    public class AddressCreatedEvent : DomainEvent
    {
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
    }
}