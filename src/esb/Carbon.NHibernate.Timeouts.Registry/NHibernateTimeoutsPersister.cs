using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Carbon.ESB.Messages;
using Carbon.ESB.Services.Impl.Timeout.Persister;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Configuration=NHibernate.Cfg.Configuration;

namespace Carbon.NHibernate.Timeouts.Registry
{
    public class NHibernateTimeoutsPersister : ITimeoutsPersister
    {
        private const string m_configuration_settings_key = "nhibernate_timeout_configuration";
        private static Configuration _configuration = null;
        private static ISessionFactory _session_factory = null;

        public NHibernateTimeoutsPersister()
        {
            Init();
        }

        public TimeoutMessage[] FindAllExpiredTimeouts()
        {
            var retval = new List<TimeoutMessage>();

            var criteria = DetachedCriteria.For<TimeoutMessage>()
                              .Add(Expression.Lt("At", System.DateTime.Now))
                              .SetResultTransformer(new DistinctRootEntityResultTransformer());

            using (var session = _session_factory.OpenSession())
            {
                var results = criteria.GetExecutableCriteria(session).List<TimeoutMessage>();
                if (results.Count() > 0)
                    retval = new List<TimeoutMessage>(results);
            }

            return retval.ToArray();
        }

        public void AbortTimeout(CancelTimeoutMessage message)
        {
            throw new NotImplementedException();
        }

        public void Save(TimeoutMessage message)
        {
            using (var session = _session_factory.OpenSession())
            using(var txn = session.BeginTransaction())
            {
                try
                {
                    session.Save(message);
                    txn.Commit();
                }
                catch (Exception exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public void Complete(TimeoutMessage message)
        {
            using (var session = _session_factory.OpenSession())
            using (var txn = session.BeginTransaction())
            {
                try
                {
                    session.Delete(message);
                    txn.Commit();
                }
                catch (Exception exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        private void Init()
        {
            if (_configuration != null) return;

            try
            {
                var configFile = ConfigurationManager.AppSettings[m_configuration_settings_key];

                if (string.IsNullOrEmpty(configFile))
                    throw new ArgumentException(string.Format("In order to use Nhibernate as the persistance store for timeouts, the application settings key of '{0}' " +
                           "should be added to the application configuration file that points to the configuration settings for NHibernate.", m_configuration_settings_key));

                _configuration = new Configuration().Configure(configFile);
                _session_factory = _configuration.BuildSessionFactory();
            }
            catch (Exception exception)
            {
                throw;
            }

        }

    }
}
