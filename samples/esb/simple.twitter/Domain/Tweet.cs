using System;
using Carbon.Core;
using Events;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB.Stereotypes.Conversations;

namespace Domain
{
    /// <summary>
    /// Domain object that will hold the information and 
    /// changes regarding a "tweet".
    /// </summary>
    [MessageEndpoint("tweet_domain_events")]
    public class Tweet : AggregateRoot,
                         ICanConsume<TweetPostedEvent>
    {
        // local state for the domain object:
        public string Message { get; set; }
        public string Who { get; set; }
        public DateTime Timestamp { get; set; }

        [InitiatedBy]
        public void Consume(TweetPostedEvent message)
        {
            // store the state on the domain object from 
            // the event:
            Message = message.Message;
            Who = message.Who;
            Timestamp = DateTime.Now;

            // record the event and increment the version:
            ApplyEvent(message);
        }
    }
}