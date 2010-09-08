using System;
using Carbon.Core.Configuration;
using Carbon.Core.Internals.Reflection;
using Carbon.ESB.Subscriptions.Persister;
using Castle.Core;
using Castle.Core.Configuration;

namespace Carbon.ESB.Subscriptions.Configuration
{
    public class SubscriptionRegistryConfiguration : AbstractElementBuilder
    {
        private const string m_element_name = "subscription-registry";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var type = configuration.Attributes["type"];
            if (string.IsNullOrEmpty(type))
                Kernel.AddComponent(typeof(ISubscriptionPersister).Name, typeof(ISubscriptionPersister),
                                    typeof(InMemorySubscriptionPersister), LifestyleType.Singleton);
            else
            {
                try
                {
                    var instance = Kernel.Resolve<IReflection>().BuildInstance(type);
                    Kernel.AddComponentInstance(typeof (ISubscriptionPersister).Name, typeof (ISubscriptionPersister),
                                                instance.GetType(), LifestyleType.Singleton);
                }
                catch (Exception exception)
                {
                    var msg =
                        string.Format(
                            "An error has occurred while attempting to build the subscription registry of '{0}'. Reason: '{1}'",
                            type, exception.Message);
                    throw new ArgumentException(msg, exception);
                }

            }

        }
    }
}