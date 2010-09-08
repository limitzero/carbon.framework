using System;
using System.Xml.Serialization;

namespace Carbon.Core.Subscription
{
    /// <summary>
    /// Contract for defining a message subscription.
    /// </summary>
    public interface ISubscription
    {
        /// <summary>
        /// (Read-Write). The identifier for the subscription instance.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// (Read-Write). The name of the channel that will receive the message.
        /// </summary>
        string Channel { get; set; }

        /// <summary>
        /// (Read-Write). The assembly qualified type name of the message that is to be processed.
        /// </summary>
        string MessageType { get; set; }

        /// <summary>
        /// (Read-Write). The assembly of the qualified type name of the message if the message type is an interface.
        /// </summary>
        string ConcreteMessageType { get; set; }

        /// <summary>
        /// (Read-Write). The name of the method on the component that will process the message.
        /// </summary>
        string MethodName { get; set; }

        /// <summary>
        /// (Read-Write). The assembly qualified type name of the component that will process the message.
        /// </summary>
        string Component { get; set; }

        /// <summary>
        /// (Read-Write). The location of where the messages, if it is a remote subscription, should be forwarded to.
        /// </summary>
        string UriLocation { get; set; }

        /// <summary>
        /// (Read-Write). The instance of the subscription message.
        /// </summary>
        [XmlIgnore] 
        object Message { get; set; }
    }
}