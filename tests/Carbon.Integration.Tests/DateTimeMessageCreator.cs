using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Stereotypes.Polled;

namespace Carbon.Integration.Tests
{
    [MessageEndpoint("create_time", "in")]
    public class DateTimeMessageCreator
    {
        //[Polled(5)]
        public string CreateMessage()
        {
            // note: if the message is generated from a component via polling, the receive pipeline will not be invoked, 
            // only the send pipeline will be invoked for polled messages (because this component is only sending messages!!!)
            return System.DateTime.Now.ToString();
        }
    }
}