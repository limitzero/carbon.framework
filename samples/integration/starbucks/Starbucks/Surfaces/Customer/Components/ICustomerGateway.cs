using Carbon.Integration.Stereotypes.Gateway;
using Starbucks.Messages;

namespace Starbucks.Surfaces.Customer.Components
{
    /// <summary>
    /// This is the gateway that will take the order from the customer
    /// and send it to the appropriate Starbucks representative for 
    /// order fulfillment (namely the "Cashier").
    /// </summary>
    public interface ICustomerGateway
    {
        /// <summary>
        /// Action for the customer to place an order.
        /// </summary>
        /// <param name="message"></param>
        [Gateway("cashier")]
        void PlaceOrder(CreateDrinkOrderMessage message);
    }
}