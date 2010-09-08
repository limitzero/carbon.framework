using System;
using Carbon.Core;
using Carbon.Integration.Stereotypes.Aggregator.Impl;
using LoanBroker.Messages;

namespace LoanBroker.Surfaces.BankingGateway.Components
{
    /// <summary>
    /// Strategy class for determing the best bank quote
    /// to send back to the client for the indicated loan.
    /// </summary>
    public class BankQuoteMessageAggregatorStrategy : 
        AbstractAggregatorMessageHandlingStrategy<BankQuoteCreatedMessage>
    {
        private static BankQuoteCreatedMessage _best_quote = null;

        public BankQuoteMessageAggregatorStrategy()
        {
            // clean up any in-process data:
            // CleanUpAction = () => { _best_quote = null; };
        }

        public override BankQuoteCreatedMessage DoAggregatorStrategy(IEnvelope message)
        {
            var payload = message.Body.GetPayload<BankQuoteCreatedMessage>();

            if (payload.ErrorCode == 0)
            {
                if (_best_quote == null)
                    _best_quote = payload;
                else
                {
                    if (payload.InterestRate < _best_quote.InterestRate)
                        _best_quote = payload;
                }
            }

            // do not return a message, we are trying to reduce the list to the best option:
            return null;
        }

        public override IEnvelope DoAggregationStrategy(IEnvelope message)
        {
            var confirmation = new BankQuoteConfirmation();

            if (_best_quote == null)
            {
                var nack = new NackMessage();
                nack.AddErrorMessages("No bank quote could be returned for the loan quote information that you requested.");
                confirmation.Error = nack;
            }
            else
            {
                var ack = new BankQuoteAck()
                              {
                                  InterestRate = _best_quote.InterestRate, 
                                  LoanAmount =  _best_quote.LoanAmount,
                                  QuoteId = _best_quote.QuoteId,
                                  SSN =  _best_quote.SSN,
                                  LoanTerm = _best_quote.LoanTerm
                              };

                confirmation.Success = ack;
            }
            return new Envelope(confirmation);
        }

    }
}