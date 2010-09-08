using System;
using System.Configuration;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Configuration;
using Carbon.ESB.Services.Impl.Timeout.Persister;

namespace Carbon.NHibernate.Timeouts.Registry.Registration
{
    public class NHibernateTimeoutPersisterRegistration  : 
        AbstractOnDemandComponentRegistration
    {
        public override void Register()
        {
            var currentImpl = Builder.Resolve(typeof (ITimeoutsPersister).Name);
            var typeToRegister = typeof (NHibernateTimeoutsPersister);

            if(currentImpl != null)
                throw new ConfigurationErrorsException("There is already a timeouts persister registered for recording timeouts. The " +
                                                       typeToRegister.FullName +
                                                       " will not be used");

            Builder.Register(typeof(ITimeoutsPersister).Name, typeof(ITimeoutsPersister), typeToRegister);

            Builder.Resolve<IAdapterMessagingTemplate>().DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                                                new Envelope("Using subscription persister of " + typeToRegister.FullName));
        }
    }
}