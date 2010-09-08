using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace LoanBroker.Messages
{
    [Message]
    public class CreditBureauRequest
    {
        public int SSN { get; set; }
    }
}
