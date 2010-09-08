using System;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Serialization;

namespace Carbon.Integration.Pipeline.Component
{
    public class SerializeMessagePipelineComponent : AbstractPipelineComponent
    {
        public SerializeMessagePipelineComponent(IObjectBuilder objectBuilder) : base(objectBuilder)
        {
        }

        public override IEnvelope Execute(IEnvelope envelope)
        {
            try
            {
                OnComponentStarted(this, envelope);

                // all messages received to this component should be registered 
                // with the serializer in order to reduce the object to its serialized
                // representation:
                var serializer = ObjectBuilder.Resolve<ISerializationProvider>();
                var message = serializer.Serialize(envelope.Body.GetPayload<object>());
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