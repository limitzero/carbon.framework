using System;
using System.Collections.Generic;
using System.Threading;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB.Configuration;
using Carbon.Test.Domain.PingPongMessages;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Xunit;
using Carbon.ESB.Messages;

namespace Carbon.ESB.Tests
{
    public class MessageConsumerTests
    {
        public static ManualResetEvent _wait = null;
        public static IList<object> _messages = null;
        public static bool _employ_wait = false;

        private IWindsorContainer _container;

        public MessageConsumerTests()
        {
            _messages = new List<object>();
            _container = new WindsorContainer(new XmlInterpreter());
            _container.Kernel.AddFacility(CarbonEsbFacility.FACILITY_ID, new CarbonEsbFacility());
        }

        ~MessageConsumerTests()
        {
            _messages.Clear();
            _messages = null;
            _container.Dispose();
        }

        [Fact]
        public void can_send_message_to_consumer_via_infrastructure()
        {
            _wait = new ManualResetEvent(false);
            _employ_wait = true;

            using(var bus = _container.Resolve<IMessageBus>())
            {
                bus.Start();

                bus.Publish(new PingMessage());
                _wait.WaitOne(TimeSpan.FromSeconds(5));

                var message = _messages[0];
                Assert.Equal(typeof(PingMessage), message.GetType());

                _wait.Reset();
            }

        }

        [Fact]
        public void can_send_a_batch_of_messages_to_consumer_via_infrastructure_and_process_each_one()
        {
            _wait = new ManualResetEvent(false);
            _employ_wait = false;

            using (var bus = _container.Resolve<IMessageBus>())
            {
                bus.Start();

                bus.Publish(new PingMessage(), 
                    new PongMessage());

                _wait.WaitOne(TimeSpan.FromSeconds(5));
                _wait.Set();

                Assert.Equal(2, _messages.Count);
                Assert.Equal(typeof(PingMessage), _messages[0].GetType());
                Assert.Equal(typeof(PongMessage), _messages[1].GetType());
            }

        }

        [Fact]
        public void can_send_a_timeout_message_to_infrastructure_to_process_message_after_a_1_minute_delay()
        {
            _wait = new ManualResetEvent(false);
            _employ_wait = true;

            using(var bus = _container.Resolve<IMessageBus>())
            {
                bus.Start();

                var msg = new TimeoutMessage(TimeSpan.FromSeconds(1), new PingMessage());
                bus.Publish(msg);

                _wait.WaitOne(TimeSpan.FromSeconds(5));

                Assert.Equal(1, _messages.Count);
                Assert.Equal(typeof(PingMessage), _messages[0].GetType());
            }
        }

        [MessageEndpoint("test")]
        public class TestMessageConsumer
        {
            public void Consume(PingMessage message)
            {
                _messages.Add(message);
                EmployWait();
             }

            public void Consume(PongMessage message)
            {
                _messages.Add(message);
            }

            private void EmployWait()
            {
                if (_employ_wait)
                    _wait.Set();
            }

        }

    }
}