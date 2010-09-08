using System.Linq;
using System.Threading;
using Carbon.Core.Builder;
using Carbon.ESB.Internals;
using Xunit;
using Carbon.Windsor.Container.Integration;
using Carbon.Test.Domain;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Adapter.Factory;

namespace Carbon.ESB.Tests
{
    public class EndpointRegistrationTests
    {
        private EndpointComponentScanner m_endpoint_scanner = null;
        private IObjectBuilder m_builder = null;

        public EndpointRegistrationTests()
        {
            m_builder = new WindsorContainerObjectBuilder();

            m_builder.Register(typeof(IObjectBuilder).Name, typeof(IObjectBuilder),
                             typeof(WindsorContainerObjectBuilder));

            m_builder.Register(typeof(IAdapterFactory).Name, typeof(IAdapterFactory),
                             typeof(AdapterFactory));

            m_builder.Register(typeof (IAdapterMessagingTemplate).Name, typeof (IAdapterMessagingTemplate),
                               typeof (AdapterMessagingTemplate));

            m_endpoint_scanner = new EndpointComponentScanner(m_builder);
        }

        [Fact]
        public void Can_scan_all_components_annotated_with_message_endpoint_attribute_and_add_to_object_builder_implementation()
        {
            m_endpoint_scanner.Scan("Carbon.Test.Domain");
            Assert.NotNull(m_builder.Resolve<InvoiceConsumer>());
            Assert.NotNull(m_endpoint_scanner.Components.FirstOrDefault(x =>x == typeof(InvoiceConsumer)));
        }
    }

 
}
