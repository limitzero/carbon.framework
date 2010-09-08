using System;
using System.Text;
using Carbon.Core;
using Carbon.Core.Builder;

namespace Carbon.Integration.Pipeline.Component
{
    public class StringToBytesPipelineComponent : AbstractPipelineComponent
    {
        public StringToBytesPipelineComponent(IObjectBuilder objectBuilder)
            : base(objectBuilder)
        {

        }

        public override IEnvelope Execute(IEnvelope envelope)
        {
            try
            {
                OnComponentStarted(this, envelope);

                if (envelope.Body.GetPayload<string>() == null)
                    throw new ArgumentException("In order to use the " + this.GetType().Name +
                                                " component, the payload of the message passed must be in string format to convert to a byte[] format. ");

                var payload = envelope.Body.GetPayload<string>();
                var message = UTF8Encoding.ASCII.GetBytes(payload);
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