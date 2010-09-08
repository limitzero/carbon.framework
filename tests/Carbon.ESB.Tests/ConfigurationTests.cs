using System;
using System.Threading;
using Carbon.ESB.Configuration;
using Carbon.ESB.Saga.Persister;
using Carbon.Test.Domain.PingPongMessages;
using Carbon.Test.Domain.Sagas;
using Carbon.Test.Domain.TimeoutMessages;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Xunit;

namespace Carbon.ESB.Tests
{
    public class ConfigurationTests
    {
        private readonly IWindsorContainer _container;

        public ConfigurationTests()
        {
            _container = new WindsorContainer(new XmlInterpreter());
            _container.Kernel.AddFacility(CarbonEsbFacility.FACILITY_ID, new CarbonEsbFacility());
        }

        ~ConfigurationTests()
        {
            _container.Dispose();
        }

        [Fact]
        public void Can_load_message_bus_from_facility()
        {
            var bus = _container.Resolve<IMessageBus>();
            Assert.NotNull(bus);
        }

        [Fact]
        public void Can_start_and_stop_message_bus_generated_from_facility()
        {
            var bus = _container.Resolve<IMessageBus>();

            bus.Start();
            Assert.True(bus.IsRunning);

            bus.Stop();

            // let the message bus do a proper shutdown:
            var wait = new ManualResetEvent(false);
            wait.WaitOne(TimeSpan.FromSeconds(5));
            wait.Set();

            Assert.False(bus.IsRunning);

        }

    }
}
