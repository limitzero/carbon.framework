using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Carbon.Core.Subscription
{
    /// <summary>
    /// Contract for building all subscriptions for messaging.
    /// </summary>
    public interface ISubscriptionBuilder
    {
        /// <summary>
        /// (Read-Only). The collection of <see cref="ISubscription">subscriptions</see> for each message to message endpoint.
        /// </summary>
        ReadOnlyCollection<ISubscription> Subscriptions { get; }

        /// <summary>
        /// This will scan an assembly, by name,  and buld all subscriptions.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to scan.</param>
        void Scan(string assemblyName);

        /// <summary>
        /// This will scan an assembly and buld all subscriptions.
        /// </summary>
        /// <param name="assembly"></param>
        void Scan(Assembly assembly);

        ISubscription[] BuildSubscriptions(Type component);
    }
}