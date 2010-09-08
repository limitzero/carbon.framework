using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Subscription;
using Carbon.ESB.Subscriptions.Persister;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Configuration=NHibernate.Cfg.Configuration;

namespace Carbon.NHibernate.Subscriptions.Registry
{
    public class NHibernateSubscriptionPersister : ISubscriptionPersister
    {
        private const string m_configuration_settings_key = "nhibernate_subscription_configuration";
        private readonly IReflection m_reflection;
        private readonly ISubscriptionBuilder m_subscription_builder;

        private bool _is_disposing = false;
        private static Configuration _configuration = null;
        private static ISessionFactory _session_factory = null;

        public NHibernateSubscriptionPersister(IReflection reflection, ISubscriptionBuilder subscriptionBuilder)
        {
            m_reflection = reflection;
            m_subscription_builder = subscriptionBuilder;
            Init();
        }

        public ReadOnlyCollection<ISubscription> GetAllItems()
        {
            var retval = new List<Subscription>();

            if (_is_disposing) return new List<ISubscription>().AsReadOnly();

            var criteria = DetachedCriteria.For<Subscription>()
                              .SetResultTransformer(new DistinctRootEntityResultTransformer());

            using (var session = _session_factory.OpenSession())
            {
                var results = criteria.GetExecutableCriteria(session).List<Subscription>();
                if (results.Count() > 0)
                    retval = new List<Subscription>(results);
            }

            var listing = new List<ISubscription>();
            foreach (var item in retval)
                listing.Add(item as ISubscription);

            return listing.AsReadOnly();

        }

        public void Register(ISubscription item)
        {
            if(_is_disposing) return;

            using(var session = _session_factory.OpenSession())
            using(var txn = session.BeginTransaction())
            {
                var sub = item as Subscription;
                var subscription = session.Get<Subscription>(item.Id);

                try
                {
                    if (subscription == null)
                        session.Save(sub);
                    txn.Commit();
                }
                catch (Exception exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public void Remove(ISubscription item)
        {
            if (_is_disposing) return;

            using (var session = _session_factory.OpenSession())
            using (var txn = session.BeginTransaction())
            {
                var sub = item as Subscription;
                var subscription = session.Get<Subscription>(item.Id);

                try
                {
                    if (subscription != null)
                        session.Delete(sub);
                    txn.Commit();
                }
                catch (Exception exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public ISubscription Find(Guid id)
        {
            ISubscription subscription = null;

            if (_is_disposing) return subscription;

            using (var session = _session_factory.OpenSession())
            {
                var result = session.Get<Subscription>(id);
                if (result != null)
                    subscription = result as Subscription;
            }

            return subscription;
        }

        public void Scan(params string[] assemblyName)
        {
            foreach (var name in assemblyName)
                try
                {
                    this.Scan(Assembly.Load(name));
                }
                catch
                {
                    continue;
                }

        }

        public void Scan(params Assembly[] assembly)
        {
            foreach (var asm in assembly)
            {
                try
                {
                    m_subscription_builder.Scan(asm);

                    foreach (var subscription in m_subscription_builder.Subscriptions)
                        this.Register(subscription);
                }
                catch
                {
                    continue;
                }
            }

        }

        public ISubscription[] Find<TMessage>(TMessage message) where TMessage : class
        {
            ISubscription[] retval = {};

            if (_is_disposing) return retval;

            var fullyQualifiedName = typeof (TMessage).AssemblyQualifiedName;

            var criteria = DetachedCriteria.For<Subscription>()
                                .Add(Expression.Eq("MessageType", fullyQualifiedName))
                                .SetResultTransformer(new DistinctRootEntityResultTransformer());

            using (var session = _session_factory.OpenSession())
            {
                var results = criteria.GetExecutableCriteria(session).List<Subscription>();
                if (results.Count() > 0)
                    retval = new List<Subscription>(results).ToArray();
            }

            return retval;
        }

        protected virtual void OnDisposing(bool disposing)
        {
            _is_disposing = disposing;

            if (_is_disposing)
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

        private static void Init()
        {
            if (_configuration != null) return;

            try
            {
                var configFile = ConfigurationManager.AppSettings[m_configuration_settings_key];

                if (string.IsNullOrEmpty(configFile))
                    throw new ArgumentException(string.Format("In order to use NHibernate as the persistance store for subscriptions, the application settings key of '{0}' " +
                           "should be added to the application configuration file that points to the configuration settings for NHibernate.", m_configuration_settings_key));

                _configuration = new Configuration().Configure(configFile);
                _session_factory = _configuration.BuildSessionFactory();
            }
            catch (Exception exception)
            {
                throw;
            }

        }



       ~NHibernateSubscriptionPersister()
        {
            OnDisposing(false);
        }
    }
}
