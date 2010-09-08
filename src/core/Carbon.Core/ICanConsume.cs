namespace Carbon.Core
{
    /// <summary>
    /// General contract applied to message consumers for one-way message consumption.
    /// </summary>
    /// <typeparam name="TMessage">Message received that will be processed</typeparam>
    public interface ICanConsume<TMessage>
    {
        /// <summary>
        /// This will consume a message and not return a response to the caller.
        /// </summary>
        /// <param name="message">Message to process</param>
        void Consume(TMessage message);
    }
}