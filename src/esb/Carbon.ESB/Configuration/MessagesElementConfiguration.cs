using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Adapter;
using Carbon.Core.Configuration;
using Carbon.ESB.Subscriptions.Persister;
using Castle.Core.Configuration;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Internals.Reflection;

namespace Carbon.ESB.Configuration
{
    public class MessagesElementConfiguration : AbstractElementBuilder
    {
        private const string m_element_name = "messages";

        public override bool IsMatchFor(string name)
        {
            return m_element_name.Trim().ToLower() == name.Trim().ToLower();
        }

        public override void Build(IConfiguration configuration)
        {
            var subscriptionPersister = Kernel.Resolve<ISubscriptionPersister>();
            var adapters = new List<AbstractInputChannelAdapter>();

            // build the set of subscriptions based on the messages namespace and uri location of delivery:
            for (var index = 0; index < configuration.Children.Count; index++ )
            {
                // <add name="{namespace or partial namespace}", uri="{location for delivery}" />
                var message = configuration.Children[index];
                var @namespace = message.Attributes["name"];
                var uri = message.Attributes["uri"];
                var concurrency = message.Attributes["concurrency"];
                var frequency = message.Attributes["frequency"];
                var scheduled = message.Attributes["scheduled"];

                foreach (var subscription in subscriptionPersister.GetAllItems())
                {
                    var toSearch = @namespace.Trim().Substring(0, @namespace.Trim().Length).Trim();
                    var currentNamespace = subscription.MessageType.Trim().Substring(0, toSearch.Length).Trim();

                    //if (subscription.MessageType.StartsWith(@namespace.Trim()))
                    if(toSearch.ToLower() == currentNamespace.ToLower())
                    {
                        subscription.UriLocation = uri;

                        var adapter = Kernel.Resolve<IAdapterFactory>()
                                   .BuildInputAdapterFromUri(subscription.Channel, uri);

                        var intValue = 0;
                        var isScheduled = false;

                        if(!string.IsNullOrEmpty(scheduled))
                            if (Int32.TryParse(scheduled, out intValue))
                            {
                                isScheduled = true;
                                adapter.Interval = intValue;
                            }

                        if (!isScheduled)
                        {
                            if (!string.IsNullOrEmpty(concurrency))
                                if (Int32.TryParse(concurrency, out intValue))
                                    adapter.Concurrency = intValue;
                                else
                                {
                                    adapter.Concurrency = 1;
                                }

                            if (!string.IsNullOrEmpty(frequency))
                                if (Int32.TryParse(frequency, out intValue))
                                    adapter.Frequency = Convert.ToInt32(intValue);
                                else
                                {
                                    adapter.Frequency = 1;
                                }
                        }

                        if (adapters.Find(x => x.ChannelName.Trim().ToLower() == subscription.Channel.Trim().ToLower()) == null)
                            adapters.Add(adapter);
                    }

                }
            }

            // register all of the input adapters for the subscriptions:
            foreach (var inputChannelAdapter in adapters)
            {
                Kernel.Resolve<IAdapterRegistry>().RegisterInputChannelAdapter(inputChannelAdapter);
            }

            // add the bus messages as well to the bus input channel adapter uri:
            var bus = Kernel.Resolve<IMessageBus>();

            foreach (var subscription in subscriptionPersister.GetAllItems())
            {
                var instance = Kernel.Resolve<IReflection>().FindTypeForName(subscription.MessageType);

                if(instance.Assembly == this.GetType().Assembly)
                {
                    subscription.UriLocation = bus.Endpoint.Uri;
                }
            } 
        }

    }
}
