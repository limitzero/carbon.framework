using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.Integration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Carbon.Integration.Dsl.Surface.Registry;
using LoanBroker.Messages;
using LoanBroker.Surfaces.LoanAcceptance.Components;


namespace LoanBroker.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            // set up the container from the application configuration:
            var container = new WindsorContainer(@"loan.broker.config.xml");
            container.Kernel.AddFacility(Carbon.Integration.Configuration.CarbonIntegrationFacility.FACILITY_ID,
                                         new Carbon.Integration.Configuration.CarbonIntegrationFacility());

            var exitCode = string.Empty;
            var wait = new ManualResetEvent(false);
            var sufaces_configured = false;

            // now start the integration context, load our surface and send the message to the source location:
            using (var context = container.Resolve<IIntegrationContext>())
            {
                try
                {
                    context.ApplicationContextError += ContextError;
                    context.LoadAllSurfaces();
                    context.Start();

                    // send the description of the processing out to the console:
                    if (!sufaces_configured)
                    {
                        foreach (var surface in context.GetComponent<ISurfaceRegistry>().Surfaces)
                        {
                            Console.WriteLine();
                            Console.WriteLine(surface.Verbalize());
                            Console.WriteLine();
                        }
                        sufaces_configured = true;

                        Console.Write("Is this configuration correct ?  (y/n):");
                        if (Console.ReadLine().Trim().ToLower() == "n")
                        {
                            context.Stop();
                            context.Dispose();
                            context.ApplicationContextError -= ContextError;
                            return;
                        }
                    }

                    while ((exitCode = GetExit()) != "q")
                    {
                        Console.Write("Enter the applicant SSN (no hyphens): ");
                        var ssn = Int32.Parse(Console.ReadLine());

                        Console.Write("Enter the loan amount: ");
                        var amount = double.Parse(Console.ReadLine());

                        Console.Write("Enter the loan terms: ");
                        var terms = Int32.Parse(Console.ReadLine());

                        Console.WriteLine("Processing your request....");

                        var gateway = context.GetComponent<ILoanQuoteGateway>();
                        var query = new LoanQuoteQuery() {LoanAmount = amount, LoanTerm = terms, SSN = ssn};
                        var confirmation = gateway.ProcessLoanQuote(query);

                        wait.WaitOne(TimeSpan.FromSeconds(5));
                        wait.Set();

                        if (confirmation.Success != null)
                            Console.WriteLine(confirmation.Success.ToString());
                        else
                        {
                            Console.WriteLine(confirmation.Error.ToString());
                        }
                    }

                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
                finally
                {
                    context.ApplicationContextError -= ContextError;
                }
            }

        
        }

        private static void ContextError(object sender, ApplicationContextErrorEventArgs e)
        {
            Console.WriteLine("Context Error : " + e.Message + " " + e.Exception);
        }

        private static string GetExit()
        {
            Console.Write("Loan Broker Host started...press 'q' to exit:");
            return Console.ReadLine();
        }
    }
}
