using System;
using System.Configuration;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Configuration;
using Carbon.ESB.Saga.Persister;

namespace Carbon.NHibernate.Saga.Registry.Registration
{
    public class NHibernateSagaPersisterRegistration  : 
        AbstractOnDemandComponentRegistration
    {
        public override void Register()
        {
            var currentImpl = Builder.Resolve(typeof(ISagaPersister<>).Name);
            var typeToRegister = typeof (NHibernateSagaPersister<>);

            if(currentImpl != null)
                throw new ConfigurationErrorsException("There is already a conversation persister registered for recording conversations. The " +
                                                       typeToRegister.FullName +
                                                       " will not be used");

            Builder.Register(typeof(ISagaPersister<>).Name, typeof(ISagaPersister<>), typeToRegister);

            Builder.Resolve<IAdapterMessagingTemplate>().DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                                                                new Envelope("Using conversation persister of " + typeToRegister.FullName));
        }
    }
}