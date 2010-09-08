using System;
using System.Collections.Generic;
using System.Threading;
using Carbon.Core;
using Carbon.ESB;
using Commands;
using Events;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Core.Builder;

namespace CommandConsumers
{
    [MessageEndpoint("post_new_tweet_command")]
    public class PostNewTweetCommandConsumer
        : ICanConsume<PostNewTweetCommand>
    {
        private readonly IMessageBus _bus = null;

        public PostNewTweetCommandConsumer(IMessageBus bus)
        {
            _bus = bus;
        }

        public void Consume(PostNewTweetCommand message)
        {
            // accept the command to post the "tweet" and publish the 
            // "tweet posted event" to the domain:
            var theEvent = new TweetPostedEvent()
                               {
                                   Message = message.Message,
                                   Who = message.Who
                               };
            _bus.Publish(theEvent);
        }
    }

    public abstract class CommandHandler<TCommand> : 
        ICanConsume<TCommand> where TCommand : class
    {
        protected CommandHandler()
        {
            
        }

        public abstract void Consume(TCommand message);
 
       
    }


    public interface IDomainEvent
    {}

    public interface IDomainRepository
    {
        T Find<T>() where T : class;
    }

    public abstract class CommandHandlerFor<TCommand> where TCommand :class
    {
        public abstract void Execute(TCommand command);
    }

    public abstract class AggregateRoot<TEvent> where TEvent : class, IDomainEvent
    {
        private readonly IObjectBuilder _builder;
        private IDictionary<Type, Action<TEvent>> _registeredEvents = new Dictionary<Type,Action<TEvent>>();

        protected AggregateRoot(IObjectBuilder builder)
        {
            _builder = builder;
            RegisterEvents();
        }

        public abstract void RegisterEvents();

        protected void Apply(TEvent domainEvent)
        {
            Action<TEvent> handler; 
            if(!_registeredEvents.TryGetValue(typeof(TEvent), out handler))
                throw new Exception();

            // apply the aggregate information to the domain event:

            // handle the event:
            handler(domainEvent);

            // send the event to the event store:
            DispatchEvent(domainEvent);
        }

        protected void RegisterEvent<TEvent>(Action<TEvent> eventHandler) where TEvent : class
        {
            _registeredEvents.Add(typeof(TEvent), theEvent => eventHandler(theEvent as TEvent));
        }

        private static void DispatchEvent(TEvent domainEvent) 
        {
            // find the event store and send the event:
            var eventStoreDenormalizer
            // find the event store and sent the aggregate root:
        }

    }

    public class PostNewTweetCommandHandler : 
        ICanConsume<PostNewTweetCommand>
    {
        private readonly IDomainRepository _repository;

        public public PostNewTweetCommandHandler(IDomainRepository repository)
        {
            _repository = repository;    
        }

        public  void Consume(PostNewTweetCommand command)
        {
            // create the event and send to the domain model:
            var domainEvent = new TweetCreatedEvent();
            var tweet = _repository.Find<Tweet>();
            tweet.MessageCreated(domainEvent);
        }
    }

    public class Tweet : AggregateRoot<IDomainEvent>
    {
        private string _message;

        public Tweet()
        {
        }

        public void MessageCreated(TweetCreatedEvent domainEvent)
        {
            // event registered here that the message was 
            // created, apply the event to the aggregate
            // and send it along to other interested parties:

            // some business logic, and then apply the event
            // (this will call the OnMessageCreated method)
            // to broadcast the event to interested parties:
            Apply(domainEvent);
        }

        public override void RegisterEvents()
        {
            RegisterEvent<TweetCreatedEvent>(OnMessageCreated);
        }

        private void OnMessageCreated(TweetCreatedEvent domainEvent)
        {
           // here we actually update the state of the domain object
            // and save the instance to storage;
            _message = domainEvent.Message;
        }

    }


    public class TweetCreatedEvent : IDomainEvent
    {
        public string Message { get; set;}
    }
}