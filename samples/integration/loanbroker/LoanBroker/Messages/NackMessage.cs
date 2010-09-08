using System;
using System.Text;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace LoanBroker.Messages
{
    [Message]
    public class NackMessage
    {
        public int SSN { get; set; }

        public string[] ErrorMessages { get; set; }

        public void AddErrorMessages(params string[] messages)
        {
            this.ErrorMessages = messages;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode()^3+ErrorMessages[0].Length;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Fault Messages:").Append(Environment.NewLine);
            foreach (var errorMessage in ErrorMessages)
                sb.Append(errorMessage).Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}