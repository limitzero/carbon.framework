using System;
using System.Text;
using Carbon.Core;
using Carbon.Core.Internals.Serialization;
using Carbon.Core.Pipeline;
using Carbon.Core.Registries.For.MessageEndpoints;

namespace Carbon.Integration.Pipeline
{
    public class IntegrationMessagingPipeline : IIntegrationMessagingPipeline
    {
        private readonly ISerializationProvider m_serialization_provider = null;
        private readonly IMessageEndpointRegistry m_message_endpoint_registry;

        public IntegrationMessagingPipeline(ISerializationProvider serializationProvider, 
                                            IMessageEndpointRegistry messageEndpointRegistry)
        {
            m_serialization_provider = serializationProvider;
            m_message_endpoint_registry = messageEndpointRegistry;
        }

        public IEnvelope Invoke(PipelineDirection direction, IEnvelope envelope)
        {
            if(direction == PipelineDirection.Receive)
                this.InvokeForReceive(envelope);

            return envelope;
        }

        private void InvokeForReceive(IEnvelope envelope)
        {
            if (m_message_endpoint_registry.GetAllItems().Count == 0)
                throw new Exception(string.Format("There was not a messsage endpoint activator found to dispatch the message '{0}'.",
                    envelope.Body.GetPayload<object>().GetType().FullName));

            foreach (var activator in m_message_endpoint_registry.GetAllItems())
            {
                if (activator.InputChannel.Name != envelope.Header.InputChannel) continue;
                activator.InvokeEndpoint(envelope);
            }
        }

        private IEnvelope PrepareMessageForDispatchToEndpoint(IEnvelope envelope)
        {
            if (envelope is NullEnvelope) new NullEnvelope();

            // message is received from the  end point, de-serialize and dispatch:
            try
            {
                if (envelope.Body.GetPayload<byte[]>() != null)
                {
                    var payload = envelope.Body.GetPayload<byte[]>();
                    var contents = new UTF8Encoding().GetString(payload);
                    var message = m_serialization_provider.Deserialize(contents);
                    envelope.Body.SetPayload(message);
                }
                else
                {
                    return envelope;
                }
            }
            catch (Exception exception)
            {
                var payload = UTF8Encoding.ASCII.GetString(envelope.Body.GetPayload<byte[]>());
                envelope.Body.SetPayload(payload);
            }

            return envelope;
        }
    }
}