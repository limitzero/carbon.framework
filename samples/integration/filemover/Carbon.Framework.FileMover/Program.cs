using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.Core;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Carbon.Integration;

namespace Carbon.Framework.FileMover
{
    class Program
    {
        static void Main(string[] args)
        {
            // set up the container from the application configuration:
            var container = new WindsorContainer(new XmlInterpreter());
            container.Kernel.AddFacility(Carbon.Integration.Configuration.CarbonIntegrationFacility.FACILITY_ID,
                                         new Carbon.Integration.Configuration.CarbonIntegrationFacility());

            var exitCode = string.Empty;
            var wait = new ManualResetEvent(false);

            while ((exitCode = GetExit()) != "q")
            {
                // prompt user for message to send:
                Console.WriteLine("Please enter a message to send:");
                var message = Console.ReadLine();

                // now start the integration context, load our surface and send the message to the source location:
                using(var context = container.Resolve<IIntegrationContext>())
                {
                    context.ApplicationContextError += ContextError;
                    context.LoadSurface("file.mover.surface"); // identifier from config...
                    context.Start();

                    // send the description of the processing out to the console:
                    var surface = context.GetComponent<FileMoverComponentSurface>();
                    Console.WriteLine(); 
                    Console.WriteLine(surface.Verbalize());
                    Console.WriteLine(); 

                    Console.Write("Is this configuration correct ?  (y/n):");
                    if(Console.ReadLine().Trim().ToLower() == "n")
                    {
                        context.Stop();
                        context.Dispose();
                        context.ApplicationContextError -= ContextError;
                        break;
                    }

                    Console.WriteLine("Sending message to: " + surface.SourceLocation);

                    // send the message to the location:
                    context.Send(new Uri(surface.SourceLocation), new Envelope(message));
                    wait.WaitOne(TimeSpan.FromSeconds(5));
                    wait.Set();

                    Console.WriteLine("Message posted to location: " + surface.TargetLocation);
                    Console.WriteLine();

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
            Console.Write("File Mover Host started...press 'q' to exit:");
            return Console.ReadLine();
        }
    }

    // this is the primary coordination point for moving files from one place to another:
}
