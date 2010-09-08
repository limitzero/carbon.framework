namespace Carbon.Core.Templates.Messaging
{
    /// <summary>
    /// Contract that will be used for sending and receiving messages
    /// </summary>
    /// <typeparam name="TLocationSemantic">The type that will identify how to communicate to a location for sending or retrieving messages.</typeparam>
    public interface IMessagingTemplate<TLocationSemantic>
    {
        /// <summary>
        /// This will transmit a message to the target location.
        /// </summary>
        /// <param name="locationSemantic">Semantic used for delivery of the message</param>
        /// <param name="message">Message to send</param>
        /// <returns></returns>
        void DoSend(TLocationSemantic locationSemantic, IEnvelope message);

        /// <summary>
        /// This will transmit a message to the target location over a given timeframe.
        /// </summary>
        /// <param name="locationSemantic">Semantic used for delivery of the message</param>
        /// <param name="message">Message to send</param>
        /// <param name="timeout">Time in seconds in which the message should be delivered.</param>
        /// <returns></returns>
        void DoSend(TLocationSemantic locationSemantic, IEnvelope message, int timeout);

        /// <summary>
        /// This will poll the target location continuously for a message to be retreived
        /// </summary>
        /// <param name="locationSemantic">Location semantic used for polling for new messages.</param>
        /// <returns>
        /// <seealso cref="IEnvelope"/>
        /// </returns>
        IEnvelope DoReceive(TLocationSemantic locationSemantic);

        /// <summary>
        /// This will poll the target location for a given time for a message to be retreived
        /// </summary>
        /// <param name="locationSemantic">Location semantic used for polling for new messages.</param>
        /// <param name="timeout">Time in seconds to poll for message.</param>
        /// <returns>
        /// <seealso cref="IEnvelope"/>
        /// </returns>
        IEnvelope DoReceive(TLocationSemantic locationSemantic, int timeout);

        /// <summary>
        /// This will perform an send and receive operation over two locations.
        /// </summary>
        /// <param name="sendLocation">Configuration in which to send the message.</param>
        /// <param name="receiveLocation">Configuration in which to receive the message.</param>
        /// <param name="message">Message to send</param>
        /// <returns>
        /// <seealso cref="IEnvelope"/>
        /// </returns>
        IEnvelope DoSendAndReceive(TLocationSemantic sendLocation, TLocationSemantic receiveLocation, IEnvelope message);

        /// <summary>
        /// This will perform an send and receive operation over two locations over a given timeframe.
        /// </summary>
        /// <param name="sendLocation">Configuration in which to send the message.</param>
        /// <param name="receiveLocation">Configuration in which to receive the message.</param>
        /// <param name="message">Message to send</param>
        /// <param name="timeout">Time in seconds in which the send and receive operation should complete.</param>
        /// <returns>
        /// <seealso cref="IEnvelope"/>
        /// </returns>
        IEnvelope DoSendAndReceive(TLocationSemantic sendLocation, TLocationSemantic receiveLocation, IEnvelope message, int timeout);
    }
}