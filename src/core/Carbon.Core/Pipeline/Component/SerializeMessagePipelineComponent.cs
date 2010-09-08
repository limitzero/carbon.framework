using System;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Serialization;

namespace Carbon.Core.Pipeline.Component
{
    public class SerializeMessagePipelineComponent : AbstractPipelineComponent
    {
        public SerializeMessagePipelineComponent(IObjectBuilder objectBuilder) : base(objectBuilder)
        {
            this.Name = "Serialize Message";
        }

        public override IEnvelope Execute(IEnvelope envelope)
        {
            try
            {
                OnComponentStarted(this, envelope);

                // all messages received to this component should be registered 
                // with the serializer in order to reduce the object to its serialized representation:
                var serializer = ObjectBuilder.Resolve<ISerializationProvider>();

                if(serializer == null)
                    throw new Exception("No component was found implementing the " + typeof(ISerializationProvider).FullName 
                        + " contract. Please add a serialization provider for serializing messages.");

                var payload = envelope.Body.GetPayload<object>();
                var message = serializer.Serialize(payload);

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