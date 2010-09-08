using Castle.MicroKernel.Facilities;
using Castle.Windsor;

namespace Carbon.Core.Configuration
{
    public class CarbonContainer : WindsorContainer
    {
        private const string m_integration_facility_name = "kharbon.integration";
        private const string m_message_bus_facility_name = "kharbon.esb";

        // facilities supported for Kharbon (two modes: integration and message bus)
        private readonly KharbonIntegrationFacility m_integration_facility = new KharbonIntegrationFacility();
        private readonly KharbonMessageBusFacility m_message_bus_facility = new KharbonMessageBusFacility();

        public CarbonContainer()
        {
            InitializeContainer();
        }

        public CarbonContainer<TFacility>(string xmlFile)
            : base(xmlFile)
        {
            InitializeContainer();
            RegisterFacility();
        }

        private static void InitializeContainer()
        {
            
        }

        private void RegisterFacility()
        {
            AddFacility(m_integration_facility_name, m_integration_facility);
            AddFacility(m_message_bus_facility_name, m_message_bus_facility);
        }
    }
}