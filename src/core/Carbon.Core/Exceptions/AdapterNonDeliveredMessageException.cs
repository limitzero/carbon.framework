using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Core.Exceptions
{
    public class AdapterNonDeliveredMessageException : ApplicationException
    {
        public string Destination { get; set; }
        public IEnvelope Envelope { get; set; }

        public AdapterNonDeliveredMessageException(string message, Exception inner, string destination, IEnvelope envelope)
            :base(message, inner)
        {
            Destination = destination;
            Envelope = envelope;
        }
    }
}
