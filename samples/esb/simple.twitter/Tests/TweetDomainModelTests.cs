using System;
using System.Linq;
using System.Threading;
using Carbon.ESB.Testing;
using Domain;
using Xunit;
using Events;
using Carbon.ESB.Saga;

namespace Tests
{
    public class when_the_event_is_sent_for_a_new_tweet_post_on_the_tweet_domain_object
        : BaseMessageBusConsumerTestFixture
    {

        private string _input_channel = string.Empty;

        public when_the_event_is_sent_for_a_new_tweet_post_on_the_tweet_domain_object()
            : base(@"simple.bus.config.xml")
        {
            _input_channel = "tweet_domain_events";
            CreateChannels(_input_channel);

            // mark the domain model for receiving event messages:
            RegisterComponent<Tweet>(_input_channel);

            // mark the domain model for persistance:
            RegisterSagaPersister<Tweet>();

            MessageBus.Start();
        }

        [Fact]
        public void then_the_event_will_be_recorded_on_the_domain_model_and_the_state_persisted()
        {
            var domainEvent = new TweetPostedEvent() { Message = "Hello", Who = "me" };
            MessageBus.Publish(domainEvent);

            var wait = new ManualResetEvent(false);
            wait.WaitOne(TimeSpan.FromSeconds(2));
            wait.Set();

            //var deliveredMessage = ReceiveMessageFromChannel<TweetPostedEvent>(_input_channel, null);

            // make sure the domain model gets the event:
            var tweetPostedEvent = (from pm in PublishedMessages
                                    where pm.GetType() == typeof(TweetPostedEvent)
                                    select pm).FirstOrDefault();
            Assert.NotNull(tweetPostedEvent);

            var sgm = ((ISagaMessage)tweetPostedEvent);
            var persister = ResolveSagaPersister<Tweet>();
            var saga = persister.Find(sgm.SagaId);

            // make sure the values are present on the saga instance:
            Assert.NotNull(saga);
            Assert.Equal(domainEvent.Message, saga.Message);
            Assert.Equal(domainEvent.Who, saga.Who);
        }
    }
}