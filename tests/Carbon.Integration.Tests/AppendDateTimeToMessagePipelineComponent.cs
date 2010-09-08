using Carbon.Core.Builder;
using Carbon.Core.Pipeline.Component;

namespace Carbon.Integration.Tests
{
    public class AppendDateTimeToMessagePipelineComponent : AbstractPipelineComponent
    {
        public AppendDateTimeToMessagePipelineComponent(IObjectBuilder objectBuilder)
            : base(objectBuilder)
        {
            Name = "Append Date Time to Message Pipeline Component";
        }

        public override Carbon.Core.IEnvelope Execute(Carbon.Core.IEnvelope envelope)
        {
            // append the current date time to the message:
            var payload = envelope.Body.GetPayload<string>();
            payload += System.DateTime.Now.ToString();
            envelope.Body.SetPayload(payload);
            return envelope;
        }
    }
}