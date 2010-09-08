using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Builder;
using Carbon.Integration.Dsl.Surface;

namespace Carbon.Test.Domain.Surfaces
{
    public class ProcessInvoiceSurface : 
        AbstractIntegrationComponentSurface
    {
        public ProcessInvoiceSurface(IObjectBuilder builder)
            : base(builder)
        {
            Name = "Process Invoice Surface";
            IsAvailable = true;
        }

        public override void BuildReceivePorts()
        {
           
        }

        public override void BuildCollaborations()
        {
            AddComponent<InvoiceConsumer>();
        }

        public override void BuildSendPorts()
        {

        }

        public override void BuildErrorPort()
        {

        }


    }
}
