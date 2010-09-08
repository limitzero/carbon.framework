using System;
using System.IO;
using System.Text;
using System.Threading;
using Carbon.Core;
using Carbon.Integration.Configuration;
using Carbon.Integration.Dsl.Surface;
using Castle.Windsor;
using Carbon.Core.Builder;
using Carbon.Core.Components;
using Carbon.Core.Pipeline.Component;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send;
using Carbon.Integration.Dsl.Surface.Ports;

using Xunit;
using Carbon.Channel.Registry;
using Carbon.Core.Channel.Impl.Queue;

namespace Carbon.Integration.Tests.Queue
{
    public class QueueAdapterIntegrationTests
    {
        private WindsorContainer _container;
        public static string _receive_channel = string.Empty;
        public static string _send_channel = string.Empty;
        public static string _error_channel = string.Empty;
        public string _uri_scheme = string.Empty;

        private static int _messages_received = 0;
        static string _receive_uri = string.Empty;
        static string _send_uri = string.Empty;
        private static string _error_uri = string.Empty;

        public QueueAdapterIntegrationTests()
        {
            _container = new WindsorContainer(@"queue/queue.config.xml");
            _container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            _uri_scheme = "vm://{0}";

            _receive_channel = "incoming";
            _send_channel = "outgoing";
            _error_channel = "error";

            // create the channels:
            _container.Resolve<IChannelRegistry>().RegisterChannel(_receive_channel);
            _container.Resolve<IChannelRegistry>().RegisterChannel(_send_channel);
            _container.Resolve<IChannelRegistry>().RegisterChannel(_error_channel);

            // create the location for storing and retrieving messages:
            _receive_uri = string.Format(_uri_scheme, _receive_channel);
            _send_uri = string.Format(_uri_scheme, _send_channel);
            _error_uri = string.Format(_uri_scheme, _error_channel);
        }

        [Fact]
        public void can_send_a_series_of_messages_to_source_via_in_memory_channel_adapter_and_move_them_to_the_target_channel()
        {
            // create some messages:
            var numberOfMessages = 5;
            for (var index = 0; index < numberOfMessages; index++)
                _container.Resolve<IChannelRegistry>().FindChannel(_send_channel).Send(new Envelope(index));

            using (var context = _container.Resolve<IIntegrationContext>())
            {
                // load our integration surface for processing messsages:
                context.LoadSurface(typeof(QueueIntegrationSurface));

                context.Start();

                var wait = new ManualResetEvent(false);
                wait.WaitOne(TimeSpan.FromSeconds(numberOfMessages * 2)); // give the adapters enough time to process...
                wait.Set();

                var inputChannel = context.GetComponent<IChannelRegistry>().FindChannel(_receive_channel) as QueueChannel;
                var outputChannel = context.GetComponent<IChannelRegistry>().FindChannel(_send_channel) as QueueChannel;

                Assert.Equal(0, inputChannel.GetMessages().Count);
                Assert.Equal(numberOfMessages, outputChannel.GetMessages().Count);

                // make sure the messages reach the component:
                Assert.Equal(numberOfMessages, _messages_received);
            }

        }

        [Fact]
        public void can_verbalize_integration_surface()
        {
            using (var context = _container.Resolve<IIntegrationContext>())
            {
                // load our integration surface for processing messsages:
                context.LoadSurface(typeof (QueueIntegrationSurface));

                context.Start();

                var surface = context.GetComponent<QueueIntegrationSurface>();
                Console.WriteLine(surface.Verbalize());
            }
        }

        public class QueuePassThroughComponent : PassThroughComponentFor<int>
        {
            public override int PassThrough(int message)
            {
                _messages_received++;
                return message;
            }
        }

        public class QueueIntegrationSurface : AbstractIntegrationComponentSurface
        {
            public QueueIntegrationSurface(IObjectBuilder builder)
                : base(builder)
            {
                this.Name = "Test Integration Surface for In-Memory Message Channel";
                this.IsAvailable = true;
            }

            public override void BuildReceivePorts()
            {
                // create ports for accepting messages:
                var uri = _receive_uri;
                CreateReceivePort(this.GetFileReceivePipeline(), "in", uri, 2, 1);
            }

            public override void BuildCollaborations()
            {
                // need component to actually handle the message when it is retrieved for processing
                // let's use the pass through component if no custom logic is to be applied:
                AddComponent<QueuePassThroughComponent>("in", "out");
            }

            public override void BuildSendPorts()
            {
                // create ports for sending messages:
                var uri = _send_uri;
                CreateSendPort(this.GetFileSendPipeline(), "out", uri, 1, 1);
            }

            public override void BuildErrorPort()
            {
                // create an error handling port where all messages will be sent:
                var uri = _error_uri;
                var config = new ErrorOutputPortConfiguration("error", uri);
                CreateErrorPort(null, config);
            }

            private AbstractReceivePipeline GetFileReceivePipeline()
            {
                return null; // not going to do any custom post-receive activities..
            }

            private AbstractSendPipeline GetFileSendPipeline()
            {
                return null; // not going to do any custom pre-send activities..
            }
        }


    }


}