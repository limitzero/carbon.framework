using System;
using System.Threading;
using Carbon.Core;
using Carbon.Core.Adapter.Factory;
using Carbon.Integration.Testing;
using Xunit;

namespace Carbon.Component.Adapter.Tests
{
    public class ComponentOutputAdapterTests : BaseMessageConsumerTestFixture
    {
        public static object _received_message = null;
        public static ManualResetEvent _wait = null;
        private string _component_id = string.Empty;
        private string _channel = string.Empty;
        private string _uri = string.Empty;

        public ComponentOutputAdapterTests()
            : base(@"empty.config.xml")
        {
            _wait = new ManualResetEvent(false);

            _component_id = "test.output.component";
            _channel = "component_output_adapter_channel";
            _uri = string.Format("direct://{0}/?method={1}&channel={2}", _component_id, "Process", _channel);

            CreateChannels(_channel);

            // must register the component in the container first before using!!!
            Container.Register(
                Castle.MicroKernel.Registration.Component.For(typeof(TestComponentForOutputAdapter))
                    .Named(_component_id));

            RegisterComponentById<TestComponentForOutputAdapter>(_component_id, _channel);
        }

        [Fact]
        public void can_create_component_output_adapter_from_uri_configuration_via_adapter_factory()
        {
            var adapter = Context.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(_uri);

            Assert.NotNull(adapter);
            Assert.Equal(typeof(ComponentOutputAdapter), adapter.GetType());
        }

        [Fact]
        public void can_generate_exception_if_no_component_can_be_resolved_from_component_id_when_starting_adapter()
        {
            var badUri = string.Format("direct://{0}/?method={1}&channel={2}", "non-resolving-component-id", "Process", _channel);
            var adapter = Context.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(_uri);
            var isErrorGenerated = false;

            // hook into the event that is triggered when the adapter has an error:
            adapter.AdapterError += (sender, args) => { isErrorGenerated = true; };

            try
            {
                adapter.Start();
                Assert.True(isErrorGenerated);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                adapter.Stop();
                adapter.AdapterError -= (sender, args) => { };
            }
        }

        [Fact]
        public void can_generate_exception_if_no_method_name_value_is_found_on_querystring_when_starting_adapter()
        {
            var badUri = string.Format("direct://{0}/?method={1}&channel={2}", _component_id, string.Empty, _channel);
            var adapter = Context.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(_uri);
            var isErrorGenerated = false;

            // hook into the event that is triggered when the adapter has an error:
            adapter.AdapterError += (sender, args) => { isErrorGenerated = true; };

            try
            {
                adapter.Start();
                Assert.True(isErrorGenerated);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                adapter.Stop();
                adapter.AdapterError -= (sender, args) => { };
            }
        }

        [Fact]
        public void can_generate_exception_if_no_channel_name_value_is_found_on_querystring_when_starting_adapter()
        {
            var badUri = string.Format("direct://{0}/?method={1}&channel={2}", _component_id, "Process", string.Empty);
            var adapter = Context.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(_uri);
            var isErrorGenerated = false;

            // hook into the event that is triggered when the adapter has an error:
            adapter.AdapterError += (sender, args) => { isErrorGenerated = true; };

            try
            {
                adapter.Start();
                Assert.True(isErrorGenerated);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                adapter.Stop();
                adapter.AdapterError -= (sender, args) => { };
            }
        }

        [Fact]
        public void can_generate_exception_if_method_consuming_message_has_no_parameters_when_starting_adapter()
        {
            var badUri = string.Format("direct://{0}/?method={1}&channel={2}", _component_id, "AcceptMessage", string.Empty);
            var adapter = Context.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(badUri);
            var isErrorGenerated = false;

            // hook into the event that is triggered when the adapter has an error:
            adapter.AdapterError += (sender, args) => { isErrorGenerated = true; };

            try
            {
                adapter.Start();
                Assert.True(isErrorGenerated);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                adapter.Stop();
                adapter.AdapterError -= (sender, args) => { };
            }
        }

        [Fact]
        public void can_consume_a_message_from_a_channel_on_the_component()
        {
            var wait = new ManualResetEvent(false);
            var adapter = Context.GetComponent<IAdapterFactory>().BuildOutputAdapterFromUri(_uri);
            var isStarted = false;

            // hook into the event that is triggered when the adapter has started:
            adapter.AdapterStarted += (sender, args) => { isStarted = true; };
            var sentMessage = "Hello";

            try
            {
                Context.Send(_channel, new Envelope(sentMessage));
                adapter.Start();
                _wait.WaitOne(TimeSpan.FromSeconds(5));

                Assert.True(isStarted);
                Assert.Equal(sentMessage, _received_message);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                adapter.Stop();
                adapter.AdapterStarted -= (sender, args) => { };
            }

        }

        // nested class for testing:
        public class TestComponentForOutputAdapter
        {
            public void Process(string message)
            {
                _received_message = message;
                _wait.Set();
            }

            public void AcceptMessage()
            {
                // this should cause the component output adapter to fail
                // since the method consuming the messages does not 
                // have any input parameters (just one!!)
            }
        }

    }
}