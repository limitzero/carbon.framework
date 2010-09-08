using Carbon.Integration.Stereotypes.Gateway;
using Carbon.Test.Domain.Messages;

namespace Carbon.Test.Domain.Gateways
{
    public interface IInvoiceProcessorGateway
    {
        [Gateway("invoices")]
        void Process(Invoice invoice);
    }
}