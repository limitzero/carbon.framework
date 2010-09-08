using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Stereotypes.Consumes;

namespace Carbon.Integration.Tests
{
    [MessageEndpoint("in2")]
    public class TestComponent2
    {
        [Consumes("out2")]
        public string Echo(string message)
        {
            return message;
        }
    }
}