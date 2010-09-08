using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Adapter.Factory;
using Carbon.Integration.Testing;
using Xunit;
using System.Threading;
using Carbon.Core.Adapter;

namespace Carbon.Component.Adapter.Tests
{
    public class ComponentInputAdapterTests : BaseMessageConsumerTestFixture
    {
        public static object _returned_message = null;
        private string _component_id = string.Empty;
        private string _channel = string.Empty;
        private string _uri = string.Empty;

        public ComponentInputAdapterTests()
            :base(@"empty.config.xml")
        {
            _component_id = "test.input.component";
            _channel = "component_input_adapter_channel";
            _uri = string.Format("direct://{0}/?method={1}&channel={2}", _component_id, "Process", _channel);

            CreateChannels(_channel);

            // must register the component in the container first before using!!!
            Container.Register(
                Castle.MicroKernel.Registration.Component.For(typeof(TestComponentForInputAdapter))
                   .Named(_component_id));

            RegisterComponentById<TestComponentForInputAdapter>(_component_id, _channel);
        }

        [Fact]
        public void can_create_component_input_adapter_from_uri_configuration_via_adapter_factory()
        {
            var adapter = Context.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(_uri);

            Assert.NotNull(adapter);
            Assert.Equal(typeof(ComponentInputAdapter), adapter.GetType());
        }

        [Fact]
        public void can_generate_exception_if_no_component_can_be_resolved_from_component_id_when_starting_adapter()
        {
            var  badUri = string.Format("direct://{0}/?method={1}&channel={2}", "non-resolving-component-id", "Process", _channel);
            var adapter = Context.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(_uri);
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
            var adapter = Context.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(_uri);
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
            var adapter = Context.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(_uri);
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
        public void can_generate_exception_if_method_producing_message_has_parameters_when_starting_adapter()
        {
            var badUri = string.Format("direct://{0}/?method={1}&channel={2}", _component_id, "GetMessage", string.Empty);
            var adapter = Context.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(badUri);
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
        public void can_receive_a_message_from_a_component_and_the_message_be_present_on_the_indicated_channel()
        {
            var wait = new ManualResetEvent(false);
            var adapter = Context.GetComponent<IAdapterFactory>().BuildInputAdapterFromUri(_uri);
            var isStarted = false;

            // hook into the event that is triggered when the adapter has started:
            adapter.AdapterStarted += (sender, args) => { isStarted = true; };

            try
            {
                adapter.Start();
                Assert.True(isStarted);

                wait.WaitOne(TimeSpan.FromSeconds(5));
                wait.Set();

                var message = ReceiveMessageFromChannel<string>(_channel, null);
                Assert.Equal("Hello", message);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                adapter.Stop();

                // can't really unsubscribe using this, but trying...
                adapter.AdapterStarted -= (sender, args) => { };
            }
        }

        // nested class for testing:
        public class TestComponentForInputAdapter
        {
            public string Process()
            {
                var message = "Hello";
                
                return message;
            }

            public string GetMessage(string message)
            {
                // this should cause the component input adapter to fail
                // since the method producing the messages should 
                // not have any input parameters:
                return message;
            }
        }

    }
}
