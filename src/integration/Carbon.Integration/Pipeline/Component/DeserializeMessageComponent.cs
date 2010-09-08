using System;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Serialization;

namespace Carbon.Integration.Pipeline.Component
{
    public class DeserializeMessageComponent : AbstractPipelineComponent
    {
        public DeserializeMessageComponent(IObjectBuilder objectBuilder)
            : base(objectBuilder)
        {
        }

        public override IEnvelope Execute(IEnvelope envelope)
        {
            try
            {
                OnComponentStarted(this, envelope);

                // all messages received from the input adapter should be in byte[] format
                // if the message is serializable, then try to generate the concrete object from 
                // the byte representation of the message:
                var serializer = ObjectBuilder.Resolve<ISerializationProvider>();
                var message = serializer.Deserialize(envelope.Body.GetPayload<byte[]>());
                envelope.Body.SetPayload(message);

                OnComponentCompleted(this, envelope);
            }
            catch (Exception exception)
            {
                if (!OnComponentError(this, envelope, exception))
                    throw;
            }

            return envelope;
        }
    }
}