using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Internals.Serialization;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Stereotypes.For.MessageHandling;

namespace Carbon.Core.Subscription
{
    /// <summary>
    /// Concrete instance for building all subscriptions for messaging.
    /// </summary>
    public class SubscriptionBuilder : ISubscriptionBuilder
    {
        private readonly ISerializationProvider m_serialization_provider;
        private List<ISubscription> m_subscriptions = null;

        /// <summary>
        /// (Read-Only). The collection of <see cref="ISubscription">subscriptions</see> for each message to message endpoint.
        /// </summary>
        public ReadOnlyCollection<ISubscription> Subscriptions { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public SubscriptionBuilder(ISerializationProvider serializationProvider)
        {
            m_serialization_provider = serializationProvider;

            if (m_subscriptions == null)
                m_subscriptions = new List<ISubscription>();
        }

        public void Scan(string assemblyName)
        {
            this.Scan(Assembly.Load(assemblyName));
        }

        public void Scan(Assembly assembly)
        {
            var endpoints = this.FindAllMessageEndpoints(assembly);

            foreach (var endpoint in endpoints)
            {
                var subscriptions = this.BuildSubscriptions(endpoint);

                foreach (var subscription in subscriptions)
                {
                    if(m_subscriptions.Find(x => x.MessageType == subscription.MessageType) == null)
                        m_subscriptions.Add(subscription);
                }
                
            }

            this.Subscriptions = m_subscriptions.AsReadOnly();
        }

        private Type[] FindAllMessageEndpoints(Assembly assembly)
        {
            var endpoints = new List<Type>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract)
                    if (type.GetCustomAttributes(typeof(MessageEndpointAttribute), true).Length > 0)
                        if (!endpoints.Contains(type))
                            endpoints.Add(type);
            }

            return endpoints.ToArray();
        }

        public ISubscription[] BuildSubscriptions(Type component)
        {
            var subscriptions = new List<ISubscription>();
            var reflection = new DefaultReflection();

            foreach (var method in component.GetMethods())
            {
                if (component.GetCustomAttributes(typeof(MessageEndpointAttribute), true).Length == 0) continue;

                var channel =
                    ((MessageEndpointAttribute)component.GetCustomAttributes(typeof(MessageEndpointAttribute), true)[0]).
                        InputChannel;

                // build a subscription for each public method and message:
                if (!method.IsPublic) continue;

                if (method.DeclaringType != component) continue;

                if (method.GetParameters().Count() != 1) continue;

                if (method.Name.StartsWith("set_")) continue;

                if (method.GetParameters().Count() == 1)
                {
                    var message = method.GetParameters()[0].ParameterType;

                    if (message == null) continue;

                    var listing = this.BuildSubscriptionListing(component, message, channel, method.Name);

                    if (listing.Count() == 0) continue;
                    subscriptions.AddRange(listing);

                    //var subscription = new Subscription()
                    //                       {
                    //                           Channel = channel,
                    //                           Component = component.AssemblyQualifiedName,
                    //                           MessageType = message.AssemblyQualifiedName,
                    //                           MethodName = method.Name
                    //                           //Message = reflection.BuildInstance(message)
                    //                       };

                    //if (!subscriptions.Contains(subscription))
                    //    subscriptions.Add(subscription);
                }
            }

            return subscriptions.ToArray();
        }

        private ISubscription[] BuildSubscriptionListing(Type component, Type message, string channel, string methodName)
        {
            var subscriptions = new List<ISubscription>();

            var method = component.GetMethod(methodName, new Type[] { message });

            var attributes = method.GetParameters()[0].GetCustomAttributes(typeof(MatchAllAttribute), true);

            if (attributes.Length > 0)
            {
                // need to create one to many mapping for messages to the single method:
                foreach (var type in m_serialization_provider.GetTypes())
                {
                    if (message.IsAssignableFrom(type))
                    {
                        var subscription = new Subscription()
                                               {
                                                   Channel = channel,
                                                   Component = component.AssemblyQualifiedName,
                                                   ConcreteMessageType = type.AssemblyQualifiedName,
                                                   MessageType = message.AssemblyQualifiedName,
                                                   MethodName = method.Name
                                               };

                        if (!subscriptions.Contains(subscription))
                            subscriptions.Add(subscription);
                    }
                }
            }
            else
            {
                var subscription = new Subscription()
                                       {
                                           Channel = channel,
                                           Component = component.AssemblyQualifiedName,
                                           MessageType = message.AssemblyQualifiedName,
                                           MethodName = method.Name
                                       };

                if (!subscriptions.Contains(subscription))
                    subscriptions.Add(subscription);
            }

            return subscriptions.ToArray();
        }

    }
}