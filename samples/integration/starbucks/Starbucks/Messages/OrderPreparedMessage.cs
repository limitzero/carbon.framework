using System.Collections.Generic;
using Carbon.Core.Stereotypes.For.Components.Message;
namespace Starbucks.Messages
{
    [Message]
    public class OrderPreparedMessage
    {
        public IList<Drink> Drinks { get; set; }
    }
}