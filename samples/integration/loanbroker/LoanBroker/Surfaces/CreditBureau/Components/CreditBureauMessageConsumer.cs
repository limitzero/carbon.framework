using System;
using Carbon.Core;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.CreditBureau.Components
{
    /// <summary>
    /// The credit bureau message consumer is responsible for accepting 
    /// any inqueries for a credit score and returning the generated credit 
    /// score for the inquiry.
    /// </summary>
    public class CreditBureauMessageConsumer
        : ICanConsumeAndReturn<CreditBureauInquiry, CreditBureauReply>
    {
        private Random _random = new Random();

        public CreditBureauReply Consume(CreditBureauInquiry message)
        {
            var creditScore = GenerateCreditProfile(message.SSN);
            creditScore.LoanAmount = message.LoanAmount;
            creditScore.LoanTerm = message.LoanTerm;

            return creditScore;
        }

        private CreditBureauReply GenerateCreditProfile(int SSN)
        {
            var creditScore = GetCreditScore(SSN);
            var creditHistory = GetHistoryLength(SSN);
            return new CreditBureauReply() {SSN = SSN, CreditScore = creditScore, HistoryLength = creditHistory};
        }

        private int GetCreditScore(int ssn)
        {
            return (int)(_random.Next(600) + 300);
        }

        private int GetHistoryLength(int SSN)
        {
            return (int)(_random.Next(19) + 1);
        }
    }
}