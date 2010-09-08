namespace Carbon.Core
{
    /// <summary>
    /// General contract applied to message consumers for bi-directional message communication.
    /// </summary>
    /// <typeparam name="TInputMessage">Message received that will be processed</typeparam>
    /// <typeparam name="TOutputMessage">Message that is to be returned by the method for processing</typeparam>
    public interface ICanConsumeAndReturn<TInputMessage, TOutputMessage>
    {
        /// <summary>
        /// This will consume a message and return the designated message type to the caller.
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <returns></returns>
        TOutputMessage Consume(TInputMessage message);
    }
}