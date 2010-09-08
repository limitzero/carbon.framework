using System;
using System.Threading;
using Carbon.Integration.Dsl.Surface.Registry;
using Carbon.Integration.Testing;
using LoanBroker.Messages;
using Xunit;
using LoanBroker.Surfaces.LoanAcceptance.Components;

namespace LoanBroker.Tests.Surfaces
{
    public class SurfaceTests : BaseMessageConsumerTestFixture
    {
        public SurfaceTests()
            :base(@"loan.broker.config.xml")
        {
            
        }

        [Fact]
        public void can_resolve_all_surfaces_and_verbalize_after_configuring()
        {
            Context.LoadAllSurfaces();

            foreach (var surface in Context.GetComponent<ISurfaceRegistry>().Surfaces)
            {
                surface.Configure();
                Console.WriteLine(surface.Verbalize());
            }
        }

        [Fact]
        public void can_send_a_loan_quote()
        {
            try
            {
                Context.LoadAllSurfaces();
                Context.Start();

                var gateway = Context.GetComponent<ILoanQuoteGateway>();
                gateway.ProcessLoanQuote(new LoanQuoteQuery() {LoanAmount = 500, LoanTerm = 5, SSN = 123456789});

                var wait = new ManualResetEvent(false);
                wait.WaitOne(TimeSpan.FromSeconds(5));
                wait.Set();

                var message =
                    ReceiveMessageFromChannel<LoanQuoteConfirmation>(
                        LoanBroker.Surfaces.LoanAcceptance.LoanBrokerComponentSurface.LOAN_QUOTE_OUTPUT_CHANNEL, null);

                Assert.NotNull(message);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Context.Stop();
            }
        }

    }
}