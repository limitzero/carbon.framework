using System.Collections.Generic;
using System.Text;

namespace Carbon.Core
{
    public interface IEnvelope
    {
        IEnvelopeHeader Header { get; set; }
        IEnvelopeBody Body { get; set; }
    }
}