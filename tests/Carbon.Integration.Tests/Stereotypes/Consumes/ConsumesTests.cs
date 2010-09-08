using System;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Integration.Configuration;
using Carbon.Integration.Stereotypes.Consumes;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Castle.Windsor;
using Xunit;
using Carbon.Core.Channel.Impl.Null;

namespace Carbon.Integration.Tests.Stereotypes.Consumes
{
    public class ConsumesTests
    {
        private IWindsorContainer _container;
        private IIntegrationContext _context;

        public ConsumesTests()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            _container = new WindsorContainer(@"empty.config.xml");
            _container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            _context = _container.Resolve<IIntegrationContext>();

            // add the channels to the infrastructure:
            _context.GetComponent<IChannelRegistry>().RegisterChannel("in");
            _context.GetComponent<IChannelRegistry>().RegisterChannel("out-1");
            _context.GetComponent<IChannelRegistry>().RegisterChannel("out-2");

            // register the component and create the message endpoint for it:
            _context.GetComponent<IObjectBuilder>().Register(typeof(DateMessageConsumer).Name, typeof(DateMessageConsumer));
            _context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint("in", string.Empty, string.Empty, new DateMessageConsumer());   
        }

        [Fact]
        public void can_send_message_to_endpoint_and_redirect_to_channel_using_consumes_attribute()
        {
            // send the message to the endpoint via the channel:
            var inputChannel = _context.GetComponent<IChannelRegistry>().FindChannel("in");
            inputChannel.Send(new Envelope(DateTime.Now));

            // wait for the message to appear on the channel:
            var wait = new ManualResetEvent(true);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            // inspect the re-directed channel as set by the "consumes" attribute:
            var outputChannel2 = _context.GetComponent<IChannelRegistry>().FindChannel("out-2");
            Assert.NotEqual(typeof(NullChannel), outputChannel2.GetType());

            var message = outputChannel2.Receive();
            Assert.Equal(typeof(Envelope), message.GetType());

            var dateTime = message.Body.GetPayload<DateTime>();
            Assert.Equal(typeof(DateTime), dateTime.GetType());
        }

    }

    [MessageEndpoint("in", "out-1")]
    public class DateMessageConsumer
    {
        [Consumes("out-2")]
        public DateTime ProcessDate(DateTime dateTime)
        {
            return dateTime;
        }
    }
}