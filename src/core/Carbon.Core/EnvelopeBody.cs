using System;

namespace Carbon.Core
{
    public class EnvelopeBody : IEnvelopeBody
    {
        public EnvelopeBody()
            : this(null)
        {
        }

        public EnvelopeBody(object payload)
        {
            if(payload is Envelope)
                throw new Exception("The payload of an envelope can not be an envelope type");

            SetPayload(payload);
        }

        public object Payload { get; private set; }

        public T GetPayload<T>()
        {
            object result = default(T);

            try
            {
                result = (T) Payload;
            }
            catch 
            {
                result = null;
            }

            return (T) result;
        }

        public void SetPayload(object payload)
        {
            Payload = payload;
        }
    }
}