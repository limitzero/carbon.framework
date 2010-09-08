using System;
using System.Collections;
using System.Collections.Generic;
using Carbon.Channel.Registry;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Configuration;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl;
using Carbon.Core.Subscription;
using Carbon.ESB.Subscriptions.Persister;
using Castle.Core.Configuration;
using Kharbon.Core.Exceptions;
using Carbon.ESB.Registries.Endpoints;

namespace Carbon.ESB.Configuration
{
    public class MessageEndpointElementConfiguration : AbstractElementBuilder
    {
        private const string m_element_name = "endpoint";

        public override bool IsMatchFor(string name)
        {
            return name.Trim().ToLower() == m_element_name;
        }

        public override void Build(IConfiguration configuration)
        {
            var value = 0;
            var endpointName = configuration.Attributes["name"];
            var inputChannel = configuration.Attributes["input-channel"];
            var outputChannel = configuration.Attributes["output-channel"];
            var endpointUri = configuration.Attributes["uri"];
            var concurrency = configuration.Attributes["concurrency"];
            var frequency = configuration.Attributes["frequency"];
            var scheduled = configuration.Attributes["scheduled"];
            var endpointRef = configuration.Attributes["ref"];

            var bus = Kernel.Resolve<IMessageBus>();

            // find the endpoint concrete instance from the reference:
            var endpoint = Kernel.Resolve(endpointRef, new Hashtable());

            // long-running conversations do not have a direct output channel
            // for sending the next message:
            if (typeof(Saga.Saga).IsAssignableFrom(endpoint.GetType()))
                outputChannel = string.Empty;

            // create the channel(s) for the endpoint:
            if(bus.IsAnnotationDriven)
                this.RegisterChannels(endpoint);
            else
            {
                this.RegisterChannels(endpoint, inputChannel, outputChannel);
            }

            // create the message endpoint activator for the endpoint instance:
            var activator = Kernel.Resolve<IMessageEndpointActivator>();
            activator.ActivationStyle = EndpointActivationStyle.ActivateOnMessageReceived;
            activator.SetInputChannel(inputChannel);
            activator.SetOutputChannel(outputChannel);
            activator.SetEndpointInstance(endpoint);
            Kernel.Resolve<IServiceBusEndpointRegistry>().Register(activator);

            // register the input and output channel adapter to the endpoint input channel (receive and send):

            #region  -- receive-side of the channel tied to the endpoint  input channel (load message from endpoint onto the channel) -- 
            if (!string.IsNullOrEmpty(endpointUri))
            {
                var inputChannelAdapter = Kernel.Resolve<IAdapterFactory>().BuildInputAdapterFromUri(inputChannel,
                                                                                                     endpointUri);

                var intValue = 0;

                if (!string.IsNullOrEmpty(scheduled))
                {
                    if (Int32.TryParse(scheduled, out intValue))
                        inputChannelAdapter.Interval = intValue;
                }
                else
                {
                    if (!string.IsNullOrEmpty(concurrency))
                        if (Int32.TryParse(concurrency, out intValue))
                            inputChannelAdapter.Concurrency = intValue;
                        else
                        {
                            inputChannelAdapter.Concurrency = 1;
                        }

                    if (!string.IsNullOrEmpty(frequency))
                        if (Int32.TryParse(frequency, out intValue))
                            inputChannelAdapter.Frequency = Convert.ToInt32(intValue);
                        else
                        {
                            inputChannelAdapter.Frequency = 1;
                        }
                }

                Kernel.Resolve<IAdapterRegistry>().RegisterInputChannelAdapter(inputChannelAdapter);
            }
            #endregion

            // configure the subscriptions to the local uri location:
            BuildSubscriptionForEndpoint(endpoint,  endpointUri);

        }

        private void RegisterChannels(object endpoint)
        {
            var attributes = endpoint.GetType().GetCustomAttributes(typeof (MessageEndpointAttribute), true);

            if(attributes.Length == 0)
                throw new MessageBusConfigurationException(string.Format("The message bus is set to be annotation driven but the endpoint '{0}' does not have the attribute '{1}' defined at the class level.", 
                                                                         endpoint.GetType().FullName, typeof(MessageEndpointAttribute).Name));

            var inputChannel = ((MessageEndpointAttribute) attributes[0]).InputChannel;
            var outputChannel = ((MessageEndpointAttribute)attributes[0]).OutputChannel;

            if (string.IsNullOrEmpty(inputChannel))
                throw new MessageBusConfigurationException(string.Format("The message bus is set to be annotation driven but the endpoint '{0}' does not have the input channel set for the attribute '{1}' which should defined at the class level.",
                                                                         endpoint.GetType().FullName, typeof(MessageEndpointAttribute).Name));
         
        }

        private void RegisterChannels(object endpoint, string inputChannel, string outputChannel)
        {
            var channelRegistry = Kernel.Resolve<IChannelRegistry>();
         
            if(string.IsNullOrEmpty(inputChannel))
                throw new MessageBusConfigurationException(string.Format("No input channel was defined for the endpoint '{0}'. Please set the value of the attribute 'input-channel' to a unique non-blank name.",
                                                                         endpoint.GetType().FullName));

            if (channelRegistry.FindChannel(inputChannel) is NullChannel)
                channelRegistry.RegisterChannel(inputChannel);

            if(!string.IsNullOrEmpty(outputChannel))
                if (channelRegistry.FindChannel(outputChannel) is NullChannel)
                    channelRegistry.RegisterChannel(outputChannel);

        }

        private void BuildSubscriptionForEndpoint(object endpoint, string endpointUri)
        {
            var builder = Kernel.Resolve<ISubscriptionBuilder>();
            var subscriptions = new List<ISubscription>(builder.BuildSubscriptions(endpoint.GetType()));
            
            if(subscriptions.Count == 0) return;

            var registry = Kernel.Resolve<ISubscriptionPersister>();

            foreach (var subscription in subscriptions)
            {
                subscription.UriLocation = endpointUri;
                registry.Register(subscription);
            }
        }

    }
}