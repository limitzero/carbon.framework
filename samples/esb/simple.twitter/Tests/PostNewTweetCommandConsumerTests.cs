using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.ESB.Testing;
using CommandConsumers;
using Xunit;
using Commands;
using Events;

namespace Tests
{
   public class when_the_command_is_sent_to_post_a_new_tweet : BaseMessageBusConsumerTestFixture
   {
       private string _input_channel = string.Empty;

       public when_the_command_is_sent_to_post_a_new_tweet()
           : base(@"simple.bus.config.xml")
       {
           _input_channel = "post_new_tweet_command";
           CreateChannels(_input_channel);

           RegisterComponent<PostNewTweetCommandConsumer>(_input_channel);
           MessageBus.Start();
       }

       [Fact]
       public void then_the_command_handler_will_receive_the_message_and_publish_a_tweet_posted_event()
       {
           var command = new PostNewTweetCommand() {Message = "Hello!", Who = "me"};
           MessageBus.Publish(command);

           // make sure that the command reaches the command consumer:
           var newTweetCommand = (from message in PublishedMessages
                                  where message.GetType() == typeof(PostNewTweetCommand)
                                  select message).FirstOrDefault();

           Assert.NotNull(newTweetCommand);

           // make sure that the "tweet posted" event is sent when a new tweet is created:
           var tweetPostedEvent = (from message in PublishedMessages
                                   where message.GetType() == typeof (TweetPostedEvent)
                                   select message).FirstOrDefault();

           Assert.NotNull(tweetPostedEvent);
       }

   }
}
