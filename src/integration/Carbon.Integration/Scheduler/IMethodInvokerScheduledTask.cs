using System.Reflection;

namespace Carbon.Integration.Scheduler
{
    /// <summary>
    /// Contract for invoking a method on a periodic basis.
    /// </summary>
    public interface IMethodInvokerScheduledTask : IScheduledTask
    {
        /// <summary>
        /// (Read-Write). The instance of the component that will be invoked.
        /// </summary>
        object Instance { get; set; }

        /// <summary>
        /// (Read-Write). The collection of messages that will be passed to the method on the component.
        /// </summary>
        object[] Messages { get; set; }

        /// <summary>
        /// (Read-Write). The method to invoke on the component.
        /// </summary>
        MethodInfo Method { get; set; }
    }
}