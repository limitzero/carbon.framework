using System;
using System.Configuration;
using Carbon.Core.Internals.Serialization;
using Carbon.ESB.Messages;
using Carbon.ESB.Saga.Persister;
using Carbon.ESB.Saga;
using NHibernate;
using NHibernate.Criterion;
using Configuration = NHibernate.Cfg.Configuration;

namespace Carbon.NHibernate.Saga.Registry
{
    public class NHibernateSagaPersister<TSaga> :
        ISagaPersister<TSaga>, IDisposable
        where TSaga : class, ISaga
    {
        private readonly ISerializationProvider _serialization_provider;
        private bool _is_disposing = false;

        private const string m_configuration_settings_key = "nhibernate_saga_configuration";
        private static Configuration _configuration = null;
        private static ISessionFactory _session_factory = null;

        public NHibernateSagaPersister(ISerializationProvider serializationProvider)
        {
            _serialization_provider = serializationProvider;
            Init();
        }

        public TSaga Find(Guid id)
        {
            var retval = default(TSaga);

            if (_is_disposing) return retval;

            try
            {
                using (var session = _session_factory.OpenSession())
                {

                    // var thread = session.Get<SagaThread>(id);

                    var criteria = DetachedCriteria.For<SagaThread>()
                        .Add(Expression.Eq("SagaId", id));

                    var thread = criteria.GetExecutableCriteria(session).UniqueResult<SagaThread>();

                    if (thread != null)
                        retval = _serialization_provider.Deserialize(thread.Saga) as TSaga;
                }
            }
            catch (Exception exception)
            {
                throw;
            }

            return retval;
        }

        public void Save(TSaga saga)
        {
            if (_is_disposing) return;

            try
            {
                using (var session = _session_factory.OpenSession())
                using (var txn = session.BeginTransaction())
                {
                    try
                    {
                        var thread = CreateSagaThread(saga);
                        session.Save(thread);
                        txn.Commit();
                    }
                    catch (Exception exception)
                    {
                        txn.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        public void Complete(Guid id)
        {

            if (_is_disposing) return;

            try
            {
                if(_session_factory == null) Init(); 

                using (var session = _session_factory.OpenSession())
                using (var txn = session.BeginTransaction())
                {
                    try
                    {
                        var thread = session.Get<SagaThread>(id);

                        if (thread != null)
                        {
                            session.Delete(thread);
                            txn.Commit();
                        }
                    }
                    catch
                    {
                        txn.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                throw;
            }
        }
        
        public void Dispose()
        {
            OnDisposing(true);
        }

        protected  virtual void OnDisposing(bool disposing)
        {
            _is_disposing = disposing;

            if(_is_disposing)
            {
                // clean up managed resources:
                if (_configuration != null)
                    _configuration = null;

                if (_session_factory != null)
                {
                    _session_factory.Close();
                    _session_factory = null;
                }
            }

            // clean up unmanaged resources:

        }

        private SagaThread CreateSagaThread(TSaga saga)
        {
            var currentSaga = saga as ISaga;
            _serialization_provider.AddType(currentSaga.GetType());
            var instance = _serialization_provider.SerializeToBytes(currentSaga);

            var ct = new SagaThread()
                         {
                             SagaId = saga.SagaId,
                             Saga = instance,
                             SagaInstance = currentSaga,
                             SagaName = saga.GetType().FullName,
                             CreateDateTime = DateTime.Now
                         };

            return ct;
        }

        private void Init()
        {
            try
            {
                if (_configuration == null)
                {
                    var configFile = ConfigurationManager.AppSettings[m_configuration_settings_key];

                    if (string.IsNullOrEmpty(configFile))
                        throw new ArgumentException(
                            string.Format(
                                "In order to use NHibernate as the persistance store for sagas, the application settings key of '{0}' " +
                                "should be added to the application configuration file that points to the configuration settings for NHibernate.",
                                m_configuration_settings_key));
                    _configuration = new Configuration().Configure(configFile);
                    _session_factory = _configuration.BuildSessionFactory();
                }
            }
            catch
            {
                throw;
            }

        }

        ~NHibernateSagaPersister()
        {
            OnDisposing(false);
        }
    }
}