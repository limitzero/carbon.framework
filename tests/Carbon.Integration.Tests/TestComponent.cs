using System;
using System.Linq;
using System.Text;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Stereotypes.Consumes;

namespace Carbon.Integration.Tests
{
    //[MessageEndpoint("in","out")]
    [MessageEndpoint("in")] // representative of a one-way channel sink
    public class TestComponent
    {
        private readonly IAdapterMessagingTemplate m_template;

        public TestComponent(IAdapterMessagingTemplate template)
        {
            m_template = template;
        }

        [Consumes("out")] // another possible way to redirect the response
        public string Echo(string message)
        {
             m_template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), new Envelope(message + " made it here!!!!"));
            return message;
        }
    }
}
