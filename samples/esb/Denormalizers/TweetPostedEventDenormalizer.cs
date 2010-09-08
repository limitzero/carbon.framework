using System;
using Carbon.Core;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Repository.Repository;
using Events;

namespace Denormalizers
{
    [MessageEndpoint("new_tweet_posted_event_denormalizer")]
    public class TweetPostedEventDenormalizer : 
        ICanConsume<TweetPostedEvent>
    {
        //private readonly IRepositoryFactory _factory;

        //public TweetPostedEventDenormalizer(IRepositoryFactory factory)
        //{
        //    _factory = factory;
        //}

        public void Consume(TweetPostedEvent domainEvent)
        {
            // take the current event message and persist the parts
            // that you need for reading later:

            /*
            var repository = _factory.CreateFor<TweetItem>();
            var item = new TweetItem() {Message = domainEvent.Message, Who = domainEvent.Who};
            repository.Persist(PersistanceAction.Save, item);
             * */
        }
    }
}