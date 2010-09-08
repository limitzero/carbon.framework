using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.ESB;
using Carbon.ESB.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;

namespace Application.Backend.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = Configure();

            using(var bus = container.Resolve<IMessageBus>())
            {
                bus.Start();
                Console.Write("Press any key to stop the application back-end host:");
                Console.Read();
            }

            container.Dispose();
        }

        private static IWindsorContainer Configure()
        {
            var container = new WindsorContainer(new XmlInterpreter());
            container.Kernel.AddFacility(CarbonEsbFacility.FACILITY_ID, new CarbonEsbFacility());

            //TODO: add in persistance for domain events (using in-memory store for right now)
            /*
             * must create the database first before attempting to persist event information!!!
            container.Kernel.Register(
                Component.For<IAutoPersistanceModel>().ImplementedBy<AutoPersistanceModel>(),
                Component.For<IRepositoryFactory>().ImplementedBy<NHibernateRepositoryFactory>()
                );
            */ 

            return container;
        }
    }
}
