namespace Carbon.Core.Stereotypes.For.MessageHandling.Gateway.Impl
{
    /// <summary>
    /// Contract for holding the configuration information for the gateway implementation.
    /// </summary>
    public interface IGatewayDefinition
    {
        /// <summary>
        /// (Read-Write). The unique instance of the gateway definition.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// (Read-Write). The interface representing the message gateway.
        /// </summary>
        string Contract { get; set; }

        /// <summary>
        /// (Read-Write). The concrete instance representing the message gateway (used for WCF gateways).
        /// </summary>
        string Service { get; set; }

        /// <summary>
        /// (Read-Write). The method to handling the message on the gateway.
        /// </summary>
        string Method { get; set; }

        /// <summary>
        /// (Read-Write). The channel that will handle the incoming message.
        /// </summary>
        string RequestChannel { get; set; }

        /// <summary>
        /// (Read-Write). The channel that will hold the response for the incoming message (sync operation).
        /// </summary>
        string ReplyChannel { get; set; }

        /// <summary>
        /// (Read-Write). Time, in seconds, to wait for the response if the reply channel is set.
        /// </summary>
        int ReceiveTimeout { get; set; }
        
    }
}