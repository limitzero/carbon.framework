using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace LoanBroker.Messages
{
    [Message]
    public class LoanQuoteReply
    {
        public int SSN { get; set; }
        public double LoanAmount { get; set; }
        public double InterestRate { get; set; }
        public string QuoteId { get; set; }
    }
}