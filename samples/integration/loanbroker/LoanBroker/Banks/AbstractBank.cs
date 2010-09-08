using System;
using Carbon.Core;
using Carbon.Core.Channel.Template;
using LoanBroker.Messages;

namespace LoanBroker.Banks
{
    /// <summary>
    /// Base class that all banks will inherit from to implement the custom logic 
    /// for generating a bank quote.
    /// </summary>
    public abstract class AbstractBank : IBank
    {
        private static int _quote_counter = 0;
        private readonly IChannelMessagingTemplate _template;

        public CreateBankQuoteMessage CurrentQuote { get; set; }

        public string ReplyTo { get; set;}

        public string InputChannel  { get; set;}

        public string Name
        {
            get; set;
        }

        public double PrimeRate
        {
            get;
            set;
        }

        public double RatePremium
        {
            get; set;
        }

        public int MaxLoanTerm
        {
            get; set;
        }

        protected AbstractBank(IChannelMessagingTemplate template)
        {
            _template = template;
        }

        public abstract bool CanHandleLoanQuoteRequest(int CreditScore, double LoanAmount, int HistoryLength);

        public virtual BankQuoteCreatedMessage Consume(CreateBankQuoteMessage message)
        {
            var quote = new BankQuoteCreatedMessage() {ErrorCode = 1, InterestRate = 0.0};
            this.CurrentQuote = message;

            if (CanHandleLoanQuoteRequest(message.CreditScore, 
                message.LoanAmount, message.HistoryLength))
                quote = this.GenerateQuote();

             // send the quote back to the bank quote accumulator for aggregation:
             _template.DoSend(message.ReplyTo, new Envelope(quote));
            
            return quote;
        }

        private BankQuoteCreatedMessage GenerateQuote()
        {
            var random = new Random();
            var quote = new BankQuoteCreatedMessage();

            if (this.CurrentQuote.LoanTerm <= MaxLoanTerm)
            {
                quote.LoanAmount = CurrentQuote.LoanAmount;
                quote.LoanTerm = CurrentQuote.LoanTerm;
                quote.SSN = CurrentQuote.SSN;

                quote.InterestRate = PrimeRate + RatePremium
                                     + (double)(CurrentQuote.LoanTerm / 12) / 10
                                     + (double)random.Next(10) / 10;
                quote.ErrorCode = 0;   
            }
            else
            {
                quote.ErrorCode = 1;
                quote.InterestRate = 0.0;
            }

            quote.QuoteId = String.Format("{0}-{1:00000}", Name, _quote_counter);
            _quote_counter++;

            return quote;
        }
    }
}