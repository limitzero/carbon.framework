using System.Reflection;

namespace Carbon.Core.Internals.MessageResolution
{
    /// <summary>
    /// Contract for resolving a message to a method on a component.
    /// </summary>
    public interface IMapMessageToMethod
    {
        /// <summary>
        /// This will return the method on a component that 
        /// matches the current <seealso cref="Envelope">message exchange</seealso>
        /// </summary>
        /// <param name="customComponent">Component that will be invoked.</param>
        /// <param name="message">Message exchange to match</param>
        /// <returns></returns>
        MethodInfo Map(object customComponent, IEnvelope message);

        /// <summary>
        /// This will return the method on a component that 
        /// matches the current message being passed.
        /// </summary>
        /// <param name="customComponent">Component that will be invoked.</param>
        /// <param name="message">Message to match</param>
        /// <returns></returns>
        MethodInfo Map(object customComponent, object message);
    }
}