using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace LoanBroker.Messages
{
    [Message]
    public class LoanQuoteAck
    {
        public int SSN { get; set; }
        public double LoanAmount { get; set; }
        public double InterestRate { get; set; }
        public string QuoteId { get; set; }

          public override int GetHashCode()
        {
            return base.GetHashCode() ^ 3 + SSN;
        }

          public override string ToString()
          {
              var sb = new StringBuilder();
              sb.Append("Quote Id: ").Append(this.QuoteId).Append(Environment.NewLine)
                  .Append("Loan Amount: ").Append(this.LoanAmount.ToString()).Append(Environment.NewLine)
                  .Append("Interest Rate: ").Append(this.InterestRate.ToString()).Append(Environment.NewLine);
              return sb.ToString();
          }
    }

    [Message]
    public class LoanQuoteConfirmation
    {
        public LoanQuoteAck Success { get; set; }
        public NackMessage Error { get; set; }
    }
}