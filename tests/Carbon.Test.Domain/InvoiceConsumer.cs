using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Integration.Stereotypes.Consumes;
using Carbon.Test.Domain.Messages;
using Carbon.Core.Adapter.Template;
using Carbon.Core;

namespace Carbon.Test.Domain
{
    [MessageEndpoint("invoices")]
    public class InvoiceConsumer
    {
        private readonly IAdapterMessagingTemplate m_template;

        public InvoiceConsumer(IAdapterMessagingTemplate template)
        {
            m_template = template;
        }

        [Consumes]
        public void AcceptInvoice(Invoice message)
        {
            m_template.DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI), 
                new Envelope("Received invoice..."));
        }
    }
}
