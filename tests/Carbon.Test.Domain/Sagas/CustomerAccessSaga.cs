using System;
using Carbon.Core.Adapter.Factory;
using Carbon.Core.Adapter.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB.Configuration;
using Carbon.ESB.Saga;
using Carbon.ESB.Saga.Persister;
using Carbon.ESB.Stereotypes.Conversations;
using Carbon.Test.Domain.Messages;

namespace Carbon.Test.Domain.Sagas
{
    public class CustomerAccessSagaBootStrapper : AbstractBootStrapper
    {
        public override bool IsMatchFor(Type component)
        {
            return component == typeof (CustomerAccessSaga);
        }

        public override void Configure()
        {
            // register the in memory saga persister:
            //Builder.Register(typeof(ISagaPersister<>).Name, typeof(ISagaPersister<>), 
            //                 typeof(InMemorySagaPersister<>), ActivationStyle.AsSingleton);

            // build the endpoint in the registry:
            //var adapter = Builder.Resolve<IAdapterFactory>().BuildInputAdapterFromUri("customerAccess", 
            //                                                                          "msmq://localhost/private$/customer_access");
            //Builder.Resolve<IAdapterRegistry>().RegisterInputChannelAdapter(adapter);
        }
    }

    [MessageEndpoint("customerAccess")]
    public class CustomerAccessSaga : Saga
    {
        [InitiatedBy]
        public void SubmitLoginRequest(LoginRequest request)
        {
            
        }
    }
}