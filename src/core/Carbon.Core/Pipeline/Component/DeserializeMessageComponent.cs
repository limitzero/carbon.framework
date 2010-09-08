using System;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Serialization;

namespace Carbon.Core.Pipeline.Component
{
    public class DeserializeMessagePipelineComponent : AbstractPipelineComponent
    {
        public DeserializeMessagePipelineComponent(IObjectBuilder objectBuilder)
            : base(objectBuilder)
        {
            this.Name = "Deserialize Message";
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

                if (serializer == null)
                    throw new Exception("No component was found implementing the " + typeof(ISerializationProvider).FullName
                        + " contract. Please add a serialization provider for de-serializing messages.");

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