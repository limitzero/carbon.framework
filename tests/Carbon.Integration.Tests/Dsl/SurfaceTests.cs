using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.Core.Builder;
using Carbon.Core.Pipeline.Component;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send;
using Carbon.Integration.Configuration;
using Carbon.Integration.Dsl.Surface;
using Carbon.Core.Stereotypes.For.Components.Message;
using Carbon.Integration.Dsl.Surface.Ports;
using Carbon.Integration.Dsl.Surface.Registry;
using Carbon.Integration.Stereotypes.Gateway;
using Castle.Windsor;
using Xunit;
using Carbon.Core.Adapter.Template;
using Carbon.Core;
using Carbon.Core.Internals.Serialization;

namespace Carbon.Integration.Tests.Dsl
{
    public class SurfaceTests
    {
        public static ManualResetEvent _wait = null;
        public static object _received_message = null;
        public static string _receive_uri = string.Empty;
        public static string _send_uri = string.Empty;
        public static string _error_uri = string.Empty;

        private IWindsorContainer _container = null;
        private IIntegrationContext _context = null;

        public SurfaceTests()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            _container = new WindsorContainer(@"empty.config.xml");
            _container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            _context = _container.Resolve<IIntegrationContext>();
            _context.ApplicationContextError += ContextError;

            _wait = new ManualResetEvent(false);

            _receive_uri = "msmq://localhost/private$/test.surface.in";
            _send_uri = @"file://C:\Work\repositories\Carbon.Framework\logs";  //"msmq://localhost/private$/test.surface.out";
            _error_uri = @"file://C:\Work\repositories\Carbon.Framework\logs";

        }

        ~SurfaceTests()
        {
            if (_context != null)
            {
                if (_context.IsRunning)
                    _context.Stop();
            }
            _context = null;

            _container.Dispose();
        }

        [Fact]
        public void can_load_integration_messaging_surface_into_context_for_processing_messages()
        {
            _context.LoadSurface<TestSurface>();
            Assert.Equal(1, _context.GetComponent<ISurfaceRegistry>().Surfaces.Count);
        }

        [Fact]
        public void can_resolve_gateway_instance_from_integration_surface()
        {
            using (var context = _container.Resolve<IIntegrationContext>())
            {
                // must start the context in order to configure all surfaces:
                context.LoadSurface<TestSurface>(); // must load the surface!!!
                context.Start();

                var gateway = _context.GetComponent<ITestMessageGateway>();
                Assert.NotNull(gateway);
            }
        }

        [Fact]
        public void can_resolve_component_instances_from_integration_surface()
        {
            using (var context = _container.Resolve<IIntegrationContext>())
            {
                // must start the context in order to configure all surfaces:
                _context.LoadSurface<TestSurface>(); // must load the surface!!!
                context.Start();

                var component1 = _context.GetComponent<TestMessageComponent1>();
                Assert.NotNull(component1);

                var component2 = _context.GetComponent<TestMessageComponent2>();
                Assert.NotNull(component2);

            }


        }

        [Fact]
        public void can_send_message_to_components_via_gateway_and_extract_a_response_message()
        {
            using (var context = _container.Resolve<IIntegrationContext>())
            {
                // must start the context in order to configure all surfaces:
                _context.LoadSurface<TestSurface>(); // must load the surface!!!
                context.Start();

                var gateway = context.GetComponent<ITestMessageGateway>();

                gateway.ProcessMessage(new TestMessage1());


                Assert.Equal(typeof(TestMessage3), _received_message.GetType());
            }
        }

        [Fact(Skip="Indeterministic on how the message is retrieved from the output location...")]
        public void can_send_message_to_components_via_one_way_gateway_and_extract_a_response_message_from_location()
        {
            using (var context = _container.Resolve<IIntegrationContext>())
            {
                // must start the context in order to configure all surfaces:
                context.LoadSurface<TestSurface>(); // must load the surface!!!
                var serializer = _context.GetComponent<ISerializationProvider>();

                context.Start();

                var surface = context.GetComponent<TestSurface>();
                Console.WriteLine(surface.Verbalize());

                var gateway = context.GetComponent<IOneWayTestMessageGateway>();

                gateway.ProcessMessage(new TestMessage1() {Contents = "This is a test message..."});

                _wait.WaitOne(TimeSpan.FromSeconds(5));
                _wait.Set();

                // retrieve the message from the location:
                var envelope = _context.GetComponent<IAdapterMessagingTemplate>().DoReceive(new Uri(_send_uri));
                Assert.NotEqual(typeof(NullEnvelope), envelope.GetType());
            }
        }

        private void ContextError(object sender, ApplicationContextErrorEventArgs e)
        {
            Console.WriteLine(e.Message + " " + e.Exception.ToString());
        }

        public class TestMessageComponent1
        {
            public TestMessage2 ProcessMessage(TestMessage1 message)
            {
                var msg = new TestMessage2() { Contents = message.Contents };
                _received_message = msg;
                return msg;
            }
        }

        public class TestMessageComponent2
        {
            public TestMessage3 ProcessMessage(TestMessage2 message)
            {
                var msg = new TestMessage3() { Contents = message.Contents };
                _received_message = msg;
                return msg;
            }
        }

        public class TestSurface : AbstractIntegrationComponentSurface
        {
            public TestSurface(IObjectBuilder builder)
                : base(builder)
            {
                // give the surface a name:
                Name = "Unit Test Component Integration Surface";

                // must mark the surface as available:
                IsAvailable = true;
            }

            public override void BuildReceivePorts()
            {
                // need to build the receive pipeline and 
                // port adapter that will look for messages:
                var uri = _receive_uri;

                // take the message from the uri and funnel it to the "message.in" channel:
                CreateReceivePort(base.ObjectBuilder.Resolve<DeserializeMessagePipeline>(), "message.in", uri, 2, 1);
            }

            public override void BuildCollaborations()
            {
                // here we add the components that will process the message:
                // (manually configure the input and output channels just to 
                // keep everything in one place):

                // add a gateway to expose message consumption to the client:
                // AddGateway<ITestMessageGateway>("ProcessMessage");

                // add a gateway to expose message consumption to the client:
                AddGateway<IOneWayTestMessageGateway>("ProcessMessage");

                // take the message from the "message.in" channel, process it and sent it to the "new_message" channel:
                AddComponent<SurfaceTests.TestMessageComponent1>("message.in", "test_message_2");
                AddComponent<SurfaceTests.TestMessageComponent2>("test_message_2", "message.out");

            }

            public override void BuildSendPorts()
            {
                // need to build the send pipeline and 
                // port adapter that will deliver messages:
                var uri = _send_uri;

                // take the message from the "message.out" channel and deliver it to  the uri specified location:
                CreateSendPort(base.ObjectBuilder.Resolve<SerializeMessagePipeline>(), "message.out", uri, 2, 1);
            }

            public override void BuildErrorPort()
            {
                // TODO: configure error port to deliver all messages to a specific location:
                // create an error handling port where all messages will be sent:
                var config = new ErrorOutputPortConfiguration("message.error", _error_uri);
                CreateErrorPort(null, config);
            }
        }
    }

    // gateway to expose consuming a message for the infrastructure:
    public interface ITestMessageGateway
    {
        [Gateway("message.in", "message.out", 10)]
        TestMessage3 ProcessMessage(TestMessage1 message);
    }

    public interface IOneWayTestMessageGateway
    {
        [Gateway("message.in")]
        void ProcessMessage(TestMessage1 message);
    }

    [Message]
    public class TestMessage1
    {
        public string Contents { get; set; }
    }

    [Message]
    public class TestMessage2
    {
        public string Contents { get; set; }
    }

    [Message]
    public class TestMessage3
    {
        public string Contents { get; set; }
    }




}
