using System;
using Carbon.Core.Stereotypes.For.Components.Message;
using Carbon.ESB.Saga;
namespace Events
{
    /// <summary>
    /// The "tweet posted" event will act as a "saga message" 
    /// that will participate in a saga for the domain model 
    /// channges.
    /// </summary>
    [Message]
    public class TweetPostedEvent : ISagaMessage
    {
        public Guid SagaId { get; set; }
        public string Who { get; set; }
        public string Message { get; set; }
    }
}