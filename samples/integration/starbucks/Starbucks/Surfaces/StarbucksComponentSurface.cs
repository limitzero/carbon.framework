using System;
using Carbon.Core.Builder;
using Carbon.Integration.Dsl.Surface;
using Starbucks.Surfaces.Customer.Components;
using Starbucks.Surfaces.Cashier.Components;
using Starbucks.Surfaces.Barrista.Components;

namespace Starbucks.Surfaces
{
	///<summary>
	/// The Starbucks component surface is the place where all of the components can 
	/// can be placed for coordinating the interactions along the different channels 
	/// and messaging transports. To see if your surface is configured correctly, 
	/// you can call the "Verbalize" method after it has been configured to see 
	/// if all of the interactions are as you expect.
	///</summary>
    public class StarbucksComponentSurface
        : AbstractIntegrationComponentSurface
    {
        public const string CUSTOMER_CHANNEL = "customer";
        public const string CASHIER_CHANNEL = "cashier";
        public const string BARRISTA_CHANNEL = "barrista";

        public StarbucksComponentSurface(IObjectBuilder builder) : base(builder)
        {
            Name = "Starbucks Component Surface";
			
			// must be set to "true" for the integration context to 
			// pick this up for registering and executing the components
            IsAvailable = true; 
        }

        public override void BuildReceivePorts()
        {
            // here is where any messages will be received from 
			// a transport location and placed onto a channel:
        }

        public override void BuildCollaborations()
        {
            AddGateway<ICustomerGateway>("PlaceOrder");
            AddComponent<CustomerMessageConsumer>(CUSTOMER_CHANNEL);
            AddComponent<CashierMessageConsumer>(CASHIER_CHANNEL);
            AddComponent<BarristaMessageConsumer>(BARRISTA_CHANNEL);
        }

        public override void BuildSendPorts()
        {
            // here is where any messages will be taken from 
			// a channel and forwarded to a physical location via the 
			// indicated transport:
        }

        public override void BuildErrorPort()
        {
           // any errors encountered by the messages in transit
		   // from receive to send ports (or vice versa) can 
		   // be sent to this physical location (if needed):
        }
    }
}