using System;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Registries.For.MessageEndpoints;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Configuration;
using Carbon.Integration.Stereotypes.Consumes;
using Carbon.Integration.Stereotypes.Gateway;
using Carbon.Integration.Stereotypes.Gateway.Configuration;
using Castle.Windsor;
using Xunit;

namespace Carbon.Integration.Tests.Stereotypes.Gateway
{
    public class GatewayTests
    {
        private IWindsorContainer _container;
        private IIntegrationContext _context;
        public static ManualResetEvent _wait = null;
        public static CarLoanApplication _received_message = null; 

        public GatewayTests()
        {
            // need to initialize the infrastructure first before the test can be conducted:
            _container = new WindsorContainer(@"empty.config.xml");
            _container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            _context = _container.Resolve<IIntegrationContext>();

            // add the channels to the infrastructure:
            _context.GetComponent<IChannelRegistry>().RegisterChannel("new_loan");
            
            // register the component and create the message endpoint for it:
            _context.GetComponent<IObjectBuilder>().Register(typeof(LoanApplicationConsumer).Name, typeof(LoanApplicationConsumer));
            var component = _context.GetComponent<LoanApplicationConsumer>();

            _context.GetComponent<IMessageEndpointRegistry>().CreateEndpoint("new_loan", string.Empty, string.Empty, component);

            // manually register the gateway (each method must be registered on the gateway):
            var gatewayBuilder = new GatewayElementBuilder();
            gatewayBuilder.Kernel = _container.Kernel;

            gatewayBuilder.Build(typeof(ICarLoanApplicationGateway), "ProcessLoan");
            gatewayBuilder.Build(typeof(IHomeLoanApplicationGateway), "ProcessLoan");
         }

        [Fact]
        public void can_retrieve_gateway_from_context_and_place_message_on_channel()
        {
            // retrieve the gateway from the context:
            var gateway = _context.GetComponent<ICarLoanApplicationGateway>(); 
            Assert.NotEqual(null, gateway);

            // send the message to the consumer via the gateway:
            gateway.ProcessLoan(new CarLoanApplication());

            var channel = _context.GetComponent<IChannelRegistry>().FindChannel("new_loan");
            Assert.NotEqual(typeof(NullChannel), channel.GetType());

            var message = channel.Receive();
            Assert.NotEqual(typeof(NullEnvelope), message.GetType());

            Assert.Equal(typeof(CarLoanApplication), message.Body.GetPayload<object>().GetType());
        }

        [Fact]
        public void can_retreive_gateway_from_context_and_forward_message_to_consumer_for_processing()
        {
            _wait = new ManualResetEvent(false);

            // retrieve the gateway from the context:
            var gateway = _context.GetComponent<ICarLoanApplicationGateway>(); 
            Assert.NotEqual(null, gateway);

            // send the message to the consumer via the gateway:
            gateway.ProcessLoan(new CarLoanApplication());
            _wait.WaitOne(TimeSpan.FromSeconds(2));

            Assert.Equal(typeof(CarLoanApplication), _received_message.GetType());
            _wait.Reset();
        }

        [Fact]
        public void can_retreive_gateway_from_context_and_forward_message_to_consumer_for_processing_and_receive_response()
        {
            _context.GetComponent<IChannelRegistry>().RegisterChannel("out");

            // retrieve the gateway from the context:
            var gateway = _context.GetComponent<IHomeLoanApplicationGateway>();
            Assert.NotEqual(null, gateway);

            // send the message to the consumer via the gateway:
            var application = gateway.ProcessLoan(new HomeLoanApplication());
            var channel = _context.GetComponent<IChannelRegistry>().FindChannel("out");

            Assert.NotEqual(typeof(NullChannel), channel.GetType());

            var message = channel.Receive();
            Assert.NotEqual(typeof(NullEnvelope), message.GetType());

            Assert.Equal(typeof(HomeLoanApplication), message.Body.GetPayload<object>().GetType());
        }

        public class CarLoanApplication
        { }

        public class HomeLoanApplication
        {}

        /// <summary>
        /// Gateway to accept a loan application message from a smart-client.
        /// </summary>
        public interface ICarLoanApplicationGateway
        {
            [Gateway("new_loan")]
            void ProcessLoan(CarLoanApplication application);
        }

        public interface IHomeLoanApplicationGateway
        {
            [Gateway("new_loan")]
            HomeLoanApplication ProcessLoan(HomeLoanApplication application);
        }

        [MessageEndpoint("new_loan")]
        public class LoanApplicationConsumer
        {
            public void AcceptCarLoan(CarLoanApplication application)
            {
                // just accept the message and don't return anything:
                _received_message = application;
                _wait.Set();
            }
            
            [Consumes("out")]
            public HomeLoanApplication AcceptHomeLoan(HomeLoanApplication application)
            {
                // echo the message back for request-response:
                return application;
            }
        }
    }
}