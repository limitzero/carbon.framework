using System;
using Carbon.Core.Registries;
using Carbon.Core.Subscription;

namespace Carbon.ESB.Subscriptions.Persister
{
    /// <summary>
    /// Contract for holding all subscriptions for message publication.
    /// </summary>
    public interface ISubscriptionPersister : IRegistry<ISubscription, Guid>
    {
        /// <summary>
        /// This will find a set of subscriptions in the subscription storage that has been or can be 
        /// associated with a messages for a single conversation.
        /// </summary>
        /// <typeparam name="TMessage">Type of the message to find the subscription</typeparam>
        /// <param name="message">Message to find the associated subscription</param>
        /// <returns></returns>
        ISubscription[] Find<TMessage>(TMessage message) where TMessage : class;
    }
}