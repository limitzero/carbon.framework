using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.Components.Message;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB.Configuration;
using Carbon.ESB.Saga;
using Carbon.ESB.Saga.Persister;
using Carbon.ESB.Stereotypes.Conversations;
using Carbon.ESB.Stereotypes.Saga;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Xunit;

namespace Carbon.ESB.Tests.Saga
{
    public class SagaTests
    {
        private IWindsorContainer _container = null;
        public static object _received_message = null;
        public static Guid _saga_id = Guid.Empty;
        public static bool _is_completed = false;

        public static ManualResetEvent _wait = null;

        public SagaTests()
        {
            _container = new WindsorContainer(new XmlInterpreter());
            _container.Kernel.AddFacility(CarbonEsbFacility.FACILITY_ID, new CarbonEsbFacility());
        }

        [Fact]
        public void Can_dispatch_message_to_initiate_saga()
        {
            _wait = new ManualResetEvent(false);
            using (var bus = _container.Resolve<IMessageBus>())
            {
                bus.Start();
                bus.Publish(new TestSagaMessage1());
                _wait.WaitOne(TimeSpan.FromSeconds(5));
                Assert.Equal(typeof (TestSagaMessage1), _received_message.GetType());
            }

        }

        [Fact]
        public void Can_send_a_message_to_initiate_a_saga_and_retreive_the_saga_instance_from_the_saga_repository()
        {
            _wait = new ManualResetEvent(false);
            using (var bus = _container.Resolve<IMessageBus>())
            {
                    bus.Start();
                    bus.Publish(new TestSagaMessage1());
                    _wait.WaitOne(TimeSpan.FromSeconds(5));

                    Assert.Equal(typeof(TestSagaMessage1), _received_message.GetType());

                    // create the persister from generic instance and resolve from container:
                    var persisterType = _container.Resolve<IReflection>()
                                    .GetGenericVersionOf(typeof(ISagaPersister<>), typeof(LocalSaga));
                    Assert.NotNull(persisterType);

                    var persister = _container.Resolve(persisterType) as ISagaPersister<LocalSaga>;
                    Assert.NotNull(persister);

                    Assert.NotEqual(Guid.Empty, _saga_id);

                    var saga = persister.Find(_saga_id);
                    Assert.NotNull(saga);
            }

            _wait.Close();
        }

        [Fact]
        public void Can_send_a_message_to_a_saga_after_it_has_been_initiated_and_retrieve_the_sage_from_the_repository()
        {
            _wait = new ManualResetEvent(false);
            using (var bus = _container.Resolve<IMessageBus>())
            {
                bus.Start();

                // initiate the saga:
                bus.Publish(new TestSagaMessage1());
                _wait.WaitOne(TimeSpan.FromSeconds(2));
                _wait.Reset();

                Assert.Equal(typeof(TestSagaMessage1), _received_message.GetType());
                Assert.NotEqual(Guid.Empty, _saga_id);

                // send message for initiated saga to continue conversation (i.e. "orchestrated by"):
                bus.Publish(new TestSagaMessage2());
                _wait.WaitOne(TimeSpan.FromSeconds(2));

                Assert.Equal(typeof(TestSagaMessage2), _received_message.GetType());

                // create the persister from generic instance and resolve from container:
                var persisterType = _container.Resolve<IReflection>()
                                .GetGenericVersionOf(typeof(ISagaPersister<>), typeof(LocalSaga));
                Assert.NotNull(persisterType);

                var persister = _container.Resolve(persisterType) as ISagaPersister<LocalSaga>;
                Assert.NotNull(persister);

                var saga = persister.Find(_saga_id);
                Assert.NotNull(saga);
            }
        }

        [Fact]
        public void Can_send_a_completion_message_to_a_saga_after_it_has_been_initiated_and_the_saga_should_be_removed_from_repository_upon_completion()
        {
            _wait = new ManualResetEvent(false);
            using (var bus = _container.Resolve<IMessageBus>())
            {
                bus.Start();

                // initiate the saga:
                bus.Publish(new TestSagaMessage1());
                _wait.WaitOne(TimeSpan.FromSeconds(2));
                _wait.Reset();

                Assert.Equal(typeof(TestSagaMessage1), _received_message.GetType());
                Assert.NotEqual(Guid.Empty, _saga_id);

                // send message for initiated saga to continue conversation (i.e. "orchestrated by"):
                bus.Publish(new TestSagaMessage2());
                _wait.WaitOne(TimeSpan.FromSeconds(2));
                _wait.Reset();

                Assert.Equal(typeof(TestSagaMessage2), _received_message.GetType());

                // create the persister from generic instance and resolve from container:
                var persisterType = _container.Resolve<IReflection>()
                                .GetGenericVersionOf(typeof(ISagaPersister<>), typeof(LocalSaga));
                Assert.NotNull(persisterType);

                var persister = _container.Resolve(persisterType) as ISagaPersister<LocalSaga>;
                Assert.NotNull(persister);

                // saga should be present:
                var saga = persister.Find(_saga_id);
                Assert.NotNull(saga);

                // send message to complete the saga:
                bus.Publish(new TestSagaCompleteMessage());
                _wait.WaitOne(TimeSpan.FromSeconds(5));
               
                // see if the flag has been set for completion:
                Assert.True(_is_completed);
                _wait.Reset();

                // saga should not be present:
                persister = _container.Resolve(persisterType) as ISagaPersister<LocalSaga>;
                saga = persister.Find(_saga_id);
                Assert.Null(saga);
            }
        }

        [Message]
        public class TestSagaMessage1 : ISagaMessage
        {
            public Guid SagaId { get; set; }
        }

        [Message]
        public class TestSagaMessage2 : ISagaMessage
        {
            public Guid SagaId { get; set; }
        }

        [Message]
        public class TestSagaCompleteMessage : ISagaMessage
        {
            public Guid SagaId { get; set; }
        }

        // set up the configuration for the saga used in testing:
        public class LocalSagaBootStrapper : AbstractBootStrapper
        {
            public override bool IsMatchFor(Type component)
            {
                return typeof (LocalSaga) == component;
            }

            public override void Configure()
            {
                // register the local in-memory saga persister for the saga (must have activation style of 
                // singleton if using in-memory persister):
                Builder.Register(typeof(ISagaPersister<LocalSaga>).FullName, typeof(ISagaPersister<LocalSaga>),
                                 typeof(InMemorySagaPersister<LocalSaga>), ActivationStyle.AsSingleton);
            }
        }

        [MessageEndpoint("local_saga")]
        public class LocalSaga : Carbon.ESB.Saga.Saga
        {
            [InitiatedBy]
            public void ProcessFirstMessage(TestSagaMessage1 message)
            {
                _received_message = message;
                _saga_id = this.SagaId;
                _wait.Set();
            }

            [OrchestratedBy]
            public void ProcessSecondMessage(TestSagaMessage2 message)
            {
                _received_message = message;
                _wait.Set();
            }

            [OrchestratedBy]
            public void CompleteSaga(TestSagaCompleteMessage message)
            {
                IsCompleted = true;
                _is_completed = IsCompleted;
                _wait.Set();
            }
        }

    }


}
