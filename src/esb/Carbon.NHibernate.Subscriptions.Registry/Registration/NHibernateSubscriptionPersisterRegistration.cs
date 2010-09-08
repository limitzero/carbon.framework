using System;
using System.Configuration;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Configuration;
using Carbon.ESB.Subscriptions.Persister;

namespace Carbon.NHibernate.Subscriptions.Registry.Registration
{
    public class NHibernateSubscriptionPersisterRegistration  : 
        AbstractOnDemandComponentRegistration
    {
        public override void Register()
        {
            var currentImpl = Builder.Resolve(typeof (ISubscriptionPersister).Name);
            var typeToRegister = typeof (NHibernateSubscriptionPersister);

            if(currentImpl != null)
                throw new ConfigurationErrorsException("There is already a subscription persister registered for recording subscriptions. The " +
                                                       typeToRegister.FullName +
                                                       " will not be used");

            Builder.Register(typeof(ISubscriptionPersister).Name, typeof(ISubscriptionPersister), typeToRegister);

            Builder.Resolve<IAdapterMessagingTemplate>().DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                                               new Envelope("Using subscription persister of " + typeToRegister.FullName));
        }
    }
}