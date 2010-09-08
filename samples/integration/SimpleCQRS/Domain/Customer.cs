using Carbon.Core.Stereotypes.For.Components.Message;
using SimpleCQRS.Commands;
using SimpleCQRS.Events;
using SimpleCQRS.Storage;

namespace SimpleCQRS.Domain
{
    [Message]
    public class Customer : BaseAggregateRoot
    {
        private readonly IDomainRepository<Customer> _repository;

        private Address _address = null;

        public Customer()
        {
        }

        public Customer(IDomainRepository<Customer> repository)
        {
            _repository = repository;
        }

        public void ClientMoved(ClientMovedCommand command)
        {
            // create the domain event for the client moving to 
            // a different address (no reason given though):
            var theEvent = new ClientMovedEvent()
                               {
                                   StreetNumber = command.StreetNumber,
                                   StreetName = command.StreetName,
                                   City = command.City,
                                   State = command.State,
                                   PostalCode = command.PostalCode
                               };
            ApplyChange(theEvent);
        }

        public override void RegisterEvents()
        {
            // register all of the events for state changes to the aggregate:
            RegisterEvent<ClientMovedEvent>(OnClientMovedEvent);
        }

        private void OnClientMovedEvent(IDomainEvent domainEvent)
        {
            var theEvent = domainEvent as ClientMovedEvent;
            
            _address = CreateEntity<Address>();
            _address = new Address(theEvent.StreetNumber, theEvent.StreetName, theEvent.City, theEvent.State,
                                   theEvent.PostalCode);

            //  push the change to the event store:
            _repository.Store(this);
        }
    }
}