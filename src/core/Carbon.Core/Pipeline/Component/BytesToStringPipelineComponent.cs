using System;
using System.Text;
using Carbon.Core.Builder;

namespace Carbon.Core.Pipeline.Component
{
    public class BytesToStringPipelineComponent : AbstractPipelineComponent
    {
        public BytesToStringPipelineComponent(IObjectBuilder objectBuilder) : base(objectBuilder)
        {
            Name = "Bytes 2 String Pipeline Component";
        }

        public override IEnvelope Execute(IEnvelope envelope)
        {
            try
            {
                OnComponentStarted(this, envelope);

                if(envelope.Body.GetPayload<byte[]>() == null)
                    throw new ArgumentException("In order to use the " + this.GetType().Name + 
                                                " component, the payload of the message passed must be in byte[] format to convert to a string. ");

                var payload = envelope.Body.GetPayload<byte[]>();
                var message = UTF8Encoding.ASCII.GetString(payload);
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