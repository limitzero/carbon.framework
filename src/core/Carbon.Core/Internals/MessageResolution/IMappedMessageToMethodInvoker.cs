namespace Carbon.Core.Internals.MessageResolution
{
    /// <summary>
    /// Contract for invoking a message mapped to a method.
    /// </summary>
    public interface IMappedMessageToMethodInvoker
    {
        /// <summary>
        /// This will invoke a method on a component an return back 
        /// the <seealso cref="IEnvelope">message</seealso>
        /// for subsequent processing for a method with no input parameters.
        /// </summary>
        IEnvelope Invoke();

        /// <summary>
        /// This will invoke a method on a component an return back 
        /// the <seealso cref="IEnvelope">message</seealso>
        /// for subsequent processing.
        /// </summary>
        /// <param name="message">Message to pass to the method for invocation.</param>
        /// <returns></returns>
        IEnvelope Invoke(object message);
       
    }
}