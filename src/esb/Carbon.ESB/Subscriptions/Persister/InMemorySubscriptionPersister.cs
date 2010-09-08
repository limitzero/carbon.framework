using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Subscription;

namespace Carbon.ESB.Subscriptions.Persister
{
    /// <summary>
    /// In-memory representation of the subscription repository.
    /// </summary>
    public class InMemorySubscriptionPersister : ISubscriptionPersister
    {
        private readonly IReflection m_reflection;
        private readonly ISubscriptionBuilder m_subscription_builder;
        private static HashSet<ISubscription> m_subscriptions = null;
        private object m_add_subscription_lock = new object();
        private object m_remove_subscription_lock = new object();
        private object m_clear_subscriptions_lock = new object();

        public InMemorySubscriptionPersister(IReflection reflection, ISubscriptionBuilder subscriptionBuilder)
        {
            m_reflection = reflection;
            m_subscription_builder = subscriptionBuilder;

            if (m_subscriptions == null)
                m_subscriptions = new HashSet<ISubscription>();
        }

        public ReadOnlyCollection<ISubscription> GetAllItems()
        {
            return new List<ISubscription>(m_subscriptions).AsReadOnly();
        }

        public void Register(ISubscription item)
        {
            if (item == null)
                return;

            try
            {
                lock (m_add_subscription_lock)
                {
                    if(!m_subscriptions.Contains(item))
                        m_subscriptions.Add(item);
                }
                
            }
            catch
            {
                // ignore dups...
            }
        }

        public void Remove(ISubscription item)
        {
            try
            {
                if (m_subscriptions.Contains(item))
                    lock (m_remove_subscription_lock)
                        m_subscriptions.Remove(item);
            }
            catch
            {
                throw;
            }
        }

        public ISubscription Find(Guid id)
        {
            ISubscription retval = null;
            ISubscription item = null;

            var subscription = from type in this.GetAllItems()
                               where type.Id == id
                               select item;

            if (subscription.Count() > 0)
                retval = subscription.ToArray()[0];

            return retval;
        }

        public void Scan(params string[] assemblyName)
        {
            foreach (var name in assemblyName)
                try
                {
                    this.Scan(Assembly.Load(name));
                }
                catch
                {
                    continue;
                }

        }

        public void Scan(params Assembly[] assembly)
        {
            foreach (var asm in assembly)
            {
                try
                {
                    //var builder = new SubscriptionBuilder();
                    m_subscription_builder.Scan(asm);

                    foreach (var subscription in m_subscription_builder.Subscriptions)
                        this.Register(subscription);
                }
                catch
                {
                    continue;
                }
            }

        }

        public ISubscription[] Find<TMessage>(TMessage message) where TMessage : class
        {
            var items = new List<ISubscription>();

            try
            {
                foreach (var subscription in m_subscriptions)
                {
                    try
                    {
                       
                        if (subscription != null)
                        {

                            // interface-based subscriptions (need to negoiate the concrete type):
                            if (!string.IsNullOrEmpty(subscription.ConcreteMessageType))
                            {
                                var instance = m_reflection.FindTypeForName(subscription.MessageType);

                                // only get the subscription for this concrete message type...not all of them for the interface:
                                if (instance.IsInterface &&
                                    instance.IsAssignableFrom(message.GetType()) && 
                                    message.GetType().AssemblyQualifiedName.Trim().ToLower() == subscription.ConcreteMessageType.Trim().ToLower())
                                    if(!items.Contains(subscription))
                                        items.Add(subscription);
                            }

                            // concrete class based subscriptions:
                            if (subscription.MessageType.Trim().ToLower() ==
                                message.GetType().AssemblyQualifiedName.Trim().ToLower())
                                if (!items.Contains(subscription))
                                    items.Add(subscription);
                        }

                       
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception exception)
            {
                throw;
            }

            return items.ToArray();
        }

        ~InMemorySubscriptionPersister()
        {
            if (m_subscriptions != null)
                lock (m_clear_subscriptions_lock)
                {
                    m_subscriptions.Clear();
                    m_subscriptions = null;
                }
        }
    }
}