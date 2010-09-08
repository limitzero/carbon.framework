namespace Carbon.Core
{
    public class Envelope : IEnvelope
    {
        public Envelope()
            : this(null)
        {
        }

        public Envelope(object payload)
        {
            Body = new EnvelopeBody(payload);
            Header = new EnvelopeHeader();
        }

        public IEnvelopeHeader Header { get; set; }

        public IEnvelopeBody Body { get; set; }
    }
}