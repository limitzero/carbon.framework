using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.Core.Builder;
using Carbon.Core.Stereotypes.For.Components.Message;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB.Configuration;
using Carbon.ESB.Messages;
using Carbon.ESB.Saga;
using Carbon.ESB.Saga.Persister;
using Carbon.ESB.Stereotypes.Conversations;
using Carbon.ESB.Stereotypes.Saga;
using Carbon.NHibernate.Saga.Registry;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using NHibernate;
using Xunit;
using nhibernate_configuration = NHibernate.Cfg.Configuration;
using nhibernate_exporter = NHibernate.Tool.hbm2ddl.SchemaExport;

namespace Carbon.ESB.NHibernate.Saga.Repository.Tests
{
    public class NHibernateSagaRepositoryTests
    {
        private IWindsorContainer _container = null;
        private nhibernate_configuration _configuration = null;
        private ISessionFactory _session_factory = null;

        public static Guid _saga_id = Guid.Empty;
        public static object _received_message = null;
        public static bool _is_completed = false;
        public static ManualResetEvent _wait = null;

        public NHibernateSagaRepositoryTests()
        {
            _container = new WindsorContainer(new XmlInterpreter());
            _container.Kernel.AddFacility(CarbonEsbFacility.FACILITY_ID, new CarbonEsbFacility());
            _saga_id = Guid.NewGuid();
        }

        ~NHibernateSagaRepositoryTests()
        {
            // clean up the resources held by the container:
            _container.Dispose();
        }

        [Fact]
        public void can_build_configuration_from_nhibernate_saga_configuration_file()
        {
            var configFile = ConfigurationManager.AppSettings["nhibernate_saga_configuration"];
            _configuration = new nhibernate_configuration();
            _configuration.Configure(configFile);
            Assert.NotNull(_configuration);
        }

        [Fact]
        public void can_build_schema_for_storing_sagas_from_configuration()
        {
            var configFile = ConfigurationManager.AppSettings["nhibernate_saga_configuration"];
            _configuration = new nhibernate_configuration();
            _configuration.Configure(configFile);

            var exporter = new nhibernate_exporter(_configuration);
            exporter.Execute(true, true, false);
        }

        [Fact]
        public void can_save_saga_instance_to_repository()
        {
            this.CreateSchema(this.GetConfiguration());

            using (var session = GetFactory(this.GetConfiguration()).OpenSession())
            using (var txn = session.BeginTransaction())
            {
                var thread = new SagaThread();

                try
                {
                    thread.Saga = ASCIIEncoding.ASCII.GetBytes("This is a sample saga instance");
                    thread.SagaName = "sample";
                    thread.SagaId = Guid.NewGuid();

                    session.Save(thread);
                    txn.Commit();
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }

                var fromDb = session.Get<SagaThread>(thread.SagaId);
                Assert.Equal(thread.SagaId, fromDb.SagaId);
            }
        }

        [Fact]
        public void can_resolve_instance_of_custom_saga_persister_from_container()
        {
            var persister = _container.Resolve(typeof(ISagaPersister<SampleNHPersistedSaga>));
            Assert.NotNull(persister);
        }

        [Fact]
        public void can_save_saga_instance_via_custom_persister()
        {
            this.CreateSchema(this.GetConfiguration());

            var persister = _container.Resolve(typeof(ISagaPersister<SampleNHPersistedSaga>))
                as ISagaPersister<SampleNHPersistedSaga>;
            Assert.NotNull(persister);

            var saga = new SampleNHPersistedSaga() { SagaId = _saga_id };
            persister.Save(saga);

            var fromPersister = persister.Find(_saga_id);
            Assert.Equal(saga.SagaId, fromPersister.SagaId);
        }

        [Fact]
        public void can_remove_saga_from_persistance_store_when_saga_is_marked_as_completed()
        {
            this.CreateSchema(this.GetConfiguration());

            var persister = _container.Resolve(typeof(ISagaPersister<SampleNHPersistedSaga>))
                as ISagaPersister<SampleNHPersistedSaga>;
            Assert.NotNull(persister);

            var saga = new SampleNHPersistedSaga() { SagaId = _saga_id };
            persister.Save(saga);

            var fromPersister = persister.Find(_saga_id);
            Assert.Equal(saga.SagaId, fromPersister.SagaId);

            persister.Complete(fromPersister.SagaId);
            Assert.Null(persister.Find(_saga_id));
        }

        [Fact]
        public void can_publish_message_to_saga_and_have_the_instance_stored_in_the_persistant_store()
        {
            this.CreateSchema(this.GetConfiguration());

            _wait = new ManualResetEvent(false);

            using (var bus = _container.Resolve<IMessageBus>())
            {
                bus.Start();

                // initiate the saga:
                bus.Publish(new SampleNHPersistedSagaMessage());
                _wait.WaitOne(TimeSpan.FromSeconds(5));

                Assert.Equal(typeof(SampleNHPersistedSagaMessage), _received_message.GetType());
                Assert.NotEqual(Guid.Empty, _saga_id);

                // the saga instance should be preserved in the persistance data store (registered in bootstrapper):
                var persister = _container.Resolve(typeof(ISagaPersister<SampleNHPersistedSaga>))
                                                  as ISagaPersister<SampleNHPersistedSaga>;

                Assert.NotNull(persister);

                var saga = persister.Find(_saga_id);
                Assert.NotNull(saga);
                Assert.Equal(typeof(SampleNHPersistedSaga), saga.GetType());
            }
        }

        [Fact]
        public void can_publish_message_to_saga_and_have_it_removed_from_persistance_store_on_completion()
        {
            this.CreateSchema(this.GetConfiguration());
            _wait = new ManualResetEvent(false);

            using (var bus = _container.Resolve<IMessageBus>())
            {
                bus.Start();

                // initiate the saga:
                bus.Publish(new SampleNHPersistedSagaMessage());
                _wait.WaitOne(TimeSpan.FromSeconds(5));
                _wait.Reset();

                Assert.Equal(typeof(SampleNHPersistedSagaMessage), _received_message.GetType());
                Assert.NotEqual(Guid.Empty, _saga_id);

                // complete the saga:
                bus.Publish(new SampleNHPersistedSagaCompleteMessage());
                _wait.WaitOne(TimeSpan.FromSeconds(5));
                _wait.Set();

                // the saga instance should be preserved in the persistance data store (registered in bootstrapper):
                var persister = _container.Resolve(typeof(ISagaPersister<SampleNHPersistedSaga>))
                                                  as ISagaPersister<SampleNHPersistedSaga>;

                Assert.NotNull(persister);

                var saga = persister.Find(_saga_id);
                Assert.Null(saga);
            }
        }

        private nhibernate_configuration GetConfiguration()
        {
            var configFile = ConfigurationManager.AppSettings["nhibernate_saga_configuration"];
            _configuration = new nhibernate_configuration();
            _configuration.Configure(configFile);
            return _configuration;
        }

        private void CreateSchema(nhibernate_configuration configuration)
        {
            var exporter = new nhibernate_exporter(configuration);
            exporter.Execute(true, true, false);
        }

        private ISessionFactory GetFactory(nhibernate_configuration configuration)
        {
            return configuration.BuildSessionFactory();
        }

        // set up the configuration for the saga used in testing:
        public class SampleNHPersistedSagaBootstrapper : AbstractBootStrapper
        {
            public override bool IsMatchFor(Type component)
            {
                return typeof(SampleNHPersistedSaga) == component;
            }

            public override void Configure()
            {
                // use NHibernate for saga persistance:
                Builder.Register(typeof(ISagaPersister<SampleNHPersistedSaga>).Name,
                    typeof(ISagaPersister<SampleNHPersistedSaga>),
                    typeof(NHibernateSagaPersister<SampleNHPersistedSaga>), 
                    ActivationStyle.AsInstance);
            }
        }

        [MessageEndpoint("nhibernate_saga")]
        public class SampleNHPersistedSaga : Carbon.ESB.Saga.Saga
        {
            [InitiatedBy]
            public void ProcessMessage(SampleNHPersistedSagaMessage message)
            {
                _received_message = message;
                _saga_id = this.SagaId;
                _wait.Set();
            }

            [OrchestratedBy]
            public void CompleteSaga(SampleNHPersistedSagaCompleteMessage message)
            {
                IsCompleted = true;
            }
        }

    }

    [Message]
    public class SampleNHPersistedSagaMessage : ISagaMessage
    {
        public Guid SagaId { get; set; }
    }

    [Message]
    public class SampleNHPersistedSagaCompleteMessage : ISagaMessage
    {
        public Guid SagaId { get; set; }
    }

}
