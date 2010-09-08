using System;
using System.Collections.Generic;
using System.Text;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Internals.Serialization;
using Carbon.Core.Pipeline;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB.Registries.Endpoints;
using System.Threading;
using Carbon.ESB.Subscriptions.Persister;

namespace Carbon.ESB.Pipeline
{
    public class MessageBusMessagingPipeline : IMessageBusMessagingPipeline
    {
        private static readonly object  m_message_lock = new object();
        private static Queue<IEnvelope> m_messages = null;
        private readonly ISerializationProvider m_serialization_provider;
        private readonly ISubscriptionPersister m_subscription_registry;
        private readonly IAdapterMessagingTemplate m_adapter_messaging_template;
        private readonly IServiceBusEndpointRegistry m_service_bus_endpoint_registry;

        public MessageBusMessagingPipeline(
            ISerializationProvider serializationProvider,
            ISubscriptionPersister subscriptionRegistry,
            IAdapterMessagingTemplate adapterMessagingTemplate,
            IServiceBusEndpointRegistry serviceBusEndpointRegistry)
        {
            m_serialization_provider = serializationProvider;
            m_subscription_registry = subscriptionRegistry;
            m_adapter_messaging_template = adapterMessagingTemplate;
            m_service_bus_endpoint_registry = serviceBusEndpointRegistry;

            if(m_messages == null)
                m_messages = new Queue<IEnvelope>();
        }

        public IEnvelope Invoke(PipelineDirection direction, IEnvelope envelope)
        {
            IEnvelope retval = new NullEnvelope();

            if (direction == PipelineDirection.Send)
                retval = this.InvokePipelineForSend(envelope);
            else
            {
                QueueMessageForReceive(envelope);
                retval = this.InvokePipelineForReceive(envelope);
                    //this.InvokePipelineForReceive(envelope);
            }

            return retval;
        }

        private IEnvelope InvokePipelineForSend(IEnvelope envelope)
        {
            var message = envelope.Body.GetPayload<object>();
            var subscriptions = m_subscription_registry.Find(message);

            if (subscriptions.Length == 0)
                throw new Exception(
                    string.Format("There was not a subscription definition defined for message '{0}'. " + 
                     "Please make sure that the component(s) that are designated as processing message(s) have the '{1}' " + 
                     "attribute declared with a unique input channel name specified.",
                                  message.GetType().FullName, 
                                  typeof(MessageEndpointAttribute).Name));

            foreach (var subscription in subscriptions)
            {
                try
                {
                    // this version of dispatch will use the uri location of the end point to 
                    // send the message content for processing:
                    envelope = this.PrepareMessageForSend(envelope, message);
                    var location = new Uri(subscription.UriLocation);
                    m_adapter_messaging_template.DoSend(location, envelope);
                }
                catch (Exception exception)
                {
                    var msg =
                        string.Format(
                            "An error has ocurred while attempting to send the message '{0}' to the location '{1}'. Reason: {2}",
                            message.GetType().FullName, subscription.UriLocation, exception.ToString());
                     m_adapter_messaging_template.DoSend(new Uri(Constants.LogUris.WARN_LOG_URI), new Envelope(msg));
                    continue;
                }
            }

            return new NullEnvelope();

        }

        private IEnvelope InvokePipelineForReceive(IEnvelope envelope)
        {
            IEnvelope retval = new NullEnvelope();

            lock (m_message_lock)
            {
                // If the queue is empty, wait for an item to be added
                // Note that this is a while loop, as we may be pulsed
                // but not wake up before another thread has come in and
                // consumed the newly added object. In that case, we'll
                // have to wait for another pulse.
                while (m_messages.Count == 0)
                {
                    // This releases message lock, only reacquiring it
                    // after being woken up by a call to Pulse
                    Monitor.Wait(m_message_lock);
                }

                retval = m_messages.Dequeue();

                retval = PrepareMessageForDispatch(retval);

                var payload = retval.Body.GetPayload<object>();

                var subscription = m_subscription_registry.Find(payload);

                if (subscription.Length == 0)
                    throw new Exception(
                        string.Format("There was not a subscription definition defined for message '{0}'",
                                      payload.GetType().FullName));

                var activator = m_service_bus_endpoint_registry.ConfigureFromSubscription(subscription[0]);

                if(activator == null)
                    throw new Exception(string.Format("No message endpoint activator was configured for component '{0}'", 
                        subscription[0].Component));

                activator.InvokeEndpoint(retval);

            }

            return retval;
        }

        private IEnvelope QueueMessageForReceive(IEnvelope envelope)
        {
            if (envelope is NullEnvelope) return new NullEnvelope(); 

            lock(m_message_lock)
            {
                if(!m_messages.Contains(envelope))
                     m_messages.Enqueue(envelope);

                // We always need to pulse, even if the queue wasn't
                // empty before. Otherwise, if we add several items
                // in quick succession, we may only pulse once, waking
                // a single thread up, even if there are multiple threads
                // waiting for items.            
                Monitor.Pulse(m_message_lock);
            }

            return new NullEnvelope();
        }

        private IEnvelope PrepareMessageForSend(IEnvelope envelope, object message)
        {
            var payload = m_serialization_provider.SerializeToBytes(message);
            envelope.Body.SetPayload(payload);
            return envelope;
        }

        private IEnvelope PrepareMessageForDispatch(IEnvelope envelope)
        {
            if (envelope is NullEnvelope) new NullEnvelope();

            // message is received from the  end point, de-serialize and dispatch:
            try
            {
                var payload = envelope.Body.GetPayload<byte[]>();
                var contents = new UTF8Encoding().GetString(payload);
                var message = m_serialization_provider.Deserialize(contents);
                envelope.Body.SetPayload(message);
            }
            catch (Exception exception)
            {
                throw;
            }

            return envelope;
        }

        ~MessageBusMessagingPipeline()
        {
            // don't loose the messages, send them back:
            foreach (var message in m_messages)
            {

                IEnvelope envelope = new NullEnvelope();

                try
                {
                    envelope = this.PrepareMessageForDispatch(message);
                }
                catch
                {
                    continue;
                }
               
                var subscriptions = m_subscription_registry.Find(envelope.Body.GetPayload<object>());

                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        m_adapter_messaging_template.DoSend(new Uri(subscription.UriLocation), envelope); 
                    }
                    catch
                    {
                        continue;         
                    }
                       
                }
            }

            m_messages.Clear();
        }

    }
}