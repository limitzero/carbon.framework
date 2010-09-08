using System;
using Carbon.Core.Stereotypes.For.Components.Message;
namespace SimpleCQRS.Commands
{


    [Message]
    public class ClientMovedCommand : Command
    {
        public string StreetName { get; set; }
        public string StreetNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
    }
}