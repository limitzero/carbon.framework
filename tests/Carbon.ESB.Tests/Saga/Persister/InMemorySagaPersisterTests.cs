using System;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.Components.Message;
using Carbon.ESB.Configuration;
using Carbon.ESB.Saga.Persister;
using Carbon.ESB.Stereotypes.Conversations;
using Carbon.ESB.Saga;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Xunit;
using Carbon.Core.Builder;

namespace Carbon.ESB.Tests.Saga.Persister
{
    public class InMemorySagaPersisterTests
    {
        private IWindsorContainer _container = null;
        private ISagaPersister<SampleInMemorySaga> _persister = null;
        private Guid _saga_id = Guid.Empty;

        public InMemorySagaPersisterTests()
        {
            _container = new WindsorContainer(new XmlInterpreter());
            _container.Kernel.AddFacility(CarbonEsbFacility.FACILITY_ID, new CarbonEsbFacility());
            _saga_id = Guid.NewGuid();
        }

        [Fact]
        public void can_resolve_instance_of_custom_saga_persister_from_container()
        {
            var persisterType = _container.Resolve<IReflection>()
                                .GetGenericVersionOf(typeof(ISagaPersister<>), typeof(SampleInMemorySaga));

            var persister = _container.Resolve(persisterType);

            Assert.NotNull(persister);
        }

        [Fact]
        public void can_save_saga_instance_via_custom_persister()
        {
            var persisterType = _container.Resolve<IReflection>()
                                .GetGenericVersionOf(typeof(ISagaPersister<>), typeof(SampleInMemorySaga));

            var persister = _container.Resolve(persisterType) as ISagaPersister<SampleInMemorySaga>;

            Assert.NotNull(persister);

            var saga = new SampleInMemorySaga() {SagaId = _saga_id};
            persister.Save(saga);

            var fromPersister = persister.Find(_saga_id);
            Assert.Equal(saga, fromPersister);
        }

        [Fact]
        public void can_remove_saga_from_persistance_store_when_saga_is_marked_as_completed()
        {
            var persisterType = _container.Resolve<IReflection>()
                                           .GetGenericVersionOf(typeof(ISagaPersister<>), typeof(SampleInMemorySaga));

            var persister = _container.Resolve(persisterType) as ISagaPersister<SampleInMemorySaga>;

            Assert.NotNull(persister);

            var saga = new SampleInMemorySaga() { SagaId = _saga_id };
            persister.Save(saga);

            var fromPersister = persister.Find(_saga_id);
            Assert.Equal(saga.SagaId, fromPersister.SagaId);

            persister.Complete(fromPersister.SagaId);
            Assert.Null(persister.Find(_saga_id));
        }

    }

    [Message]
    public class SamplePersistedSagaMessage : ISagaMessage
    {
        public Guid SagaId { get; set;}
    }

    public class SampleInMemorySagaBootstrapper : AbstractBootStrapper
    {
        public override bool IsMatchFor(Type component)
        {
            return typeof (SampleInMemorySaga) == component;
        }

        public override void Configure()
        {
            Builder.Register( typeof(ISagaPersister<SampleInMemorySaga>).FullName, 
                typeof(ISagaPersister<SampleInMemorySaga>), 
                typeof(InMemorySagaPersister<SampleInMemorySaga>), ActivationStyle.AsSingleton);
        }
    }

    public class SampleInMemorySaga : Carbon.ESB.Saga.Saga
    {
        [InitiatedBy]
        public void ProcessMessage(object message)
        {
            // do nothing...
        }
    }
}