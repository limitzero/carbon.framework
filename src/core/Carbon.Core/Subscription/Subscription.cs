using System;
using System.Xml.Serialization;

namespace Carbon.Core.Subscription
{
    /// <summary>
    /// Concrete instance for defining a message subscription.
    /// </summary>
    public class Subscription : ISubscription
    {
        public Guid Id { get; set; }
        public string Channel { get; set; }
        public string MessageType { get; set; }
        public string ConcreteMessageType { get; set; }
        public string MethodName { get; set; }
        public string Component { get; set; }
        public string UriLocation { get; set; }

        [XmlIgnore]
        public object Message { get; set; } 
    }
}