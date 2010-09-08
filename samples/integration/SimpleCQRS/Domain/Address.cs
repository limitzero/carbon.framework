using System;
using System.Text;
using Carbon.Core.Stereotypes.For.Components.Message;
using SimpleCQRS.Events;

namespace SimpleCQRS.Domain
{
    [Message]
    public class Address : BaseDomainEntity
    {
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        public Address()
            : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        public Address(string streetNumber, string streetName, string city, string state, string postalCode)
        {
            StreetNumber = streetNumber;
            StreetName = streetName;
            City = city;
            State = state;
            PostalCode = postalCode;

            var theEvent = new AddressCreatedEvent()
            {
                StreetNumber = StreetNumber,
                StreetName = StreetName,
                City = City,
                State = State,
                PostalCode = PostalCode
            };

            ApplyChange(theEvent);
        }

        public override void RegisterEvents()
        {
            RegisterEvent<AddressCreatedEvent>(OnAddressCreated);
        }

        public override string ToString()
        {
            var s = new StringBuilder();

            s.AppendLine(base.ToString());

            try
            {
                s.AppendLine(string.Format("Street Number: {0}", StreetNumber));
                s.AppendLine(string.Format("Street Name: {0}", StreetName));
                s.AppendLine(string.Format("City: {0}", City));
                s.AppendLine(string.Format("State: {0}", State));
                s.AppendLine(string.Format("Postal Code: {0}", PostalCode));
            }
            catch (Exception e)
            {
                s.AppendLine(string.Format("Error in ToString(): {0}", e));
            }
            return s.ToString();
        }

        private void OnAddressCreated(IDomainEvent domainEvent)
        {

        }

    }
}