using System;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core;
using Carbon.Core.Builder;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Internals.Dispatcher;
using Carbon.Core.Internals.Reflection;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.ESB.Saga.Persister;
using Carbon.ESB.Saga;
using Carbon.ESB.Stereotypes.Conversations;
using Carbon.Core.Internals.MessageResolution;
using Castle.Core;

namespace Carbon.ESB.Stereotypes.Saga.Impl
{
    /// <summary>
    /// Concrete strategy for handling long-running conversations for a message endpoint.
    /// </summary>
    public class SagaMessageHandlingStrategy : IServiceBusMessageHandlingStrategy
    {
        private IObjectBuilder m_container = null;

        public event EventHandler<MessageHandlingStrategyCompleteEventArgs> ChannelStrategyCompleted;

        public IMessageBus Bus { get; set; }
        public AbstractChannel InputChannel { get; private set; }
        public AbstractChannel OutputChannel { get; private set; }
        public MethodInfo CurrentMethod { get; private set; }
        public object CurrentInstance { get; private set; }

        public void SetContext(IObjectBuilder context)
        {
            this.m_container = context;
        }

        /// <summary>
        /// This will set the input channel for the service activated component for receiving a message.
        /// </summary>
        /// <param name="name">The name of the channel</param>
        public void SetInputChannel(string name)
        {
            var channel = m_container.Resolve<IChannelRegistry>().FindChannel(name);

            if (!(channel is NullChannel))
                SetInputChannel(channel);
        }

        /// <summary>
        /// This will set the input channel for the message activated component for receiving a message.
        /// </summary>
        /// <param name="channel">The channel that will hold the contents for the message to be processed.</param>
        public void SetInputChannel(AbstractChannel channel)
        {
            this.InputChannel = channel;
        }

        /// <summary>
        /// This will set the output channel where the message will be delivered after processing.
        /// </summary>
        /// <param name="name"></param>
        public void SetOutputChannel(string name)
        {
            var channel = m_container.Resolve<IChannelRegistry>().FindChannel(name);

            if (!(channel is NullChannel))
                SetOutputChannel(channel);
        }

        /// <summary>
        /// This will set the output channel where the message will be delivered after processing.
        /// </summary>
        /// <param name="channel">The channel that will hold the contents of the processed message.</param>
        public void SetOutputChannel(AbstractChannel channel)
        {
            this.OutputChannel = channel;
        }

        /// <summary>
        /// This will set the value of the current instance where the method that is implementing the channel strategy is located.
        /// </summary>
        /// <param name="instance"></param>
        public void SetInstance(object instance)
        {
            this.CurrentInstance = instance;
        }

        /// <summary>
        /// This will set the value of the current method that is implementing the channel strategy.
        /// </summary>
        /// <param name="method"></param>
        public void SetMethod(MethodInfo method)
        {
            CurrentMethod = method;
        }

        /// <summary>
        /// This will set the value of the current method that is implementing the channel strategy.
        /// </summary>
        /// <param name="name">Name of the method on the instance that will be executing the strategy.</param>
        public void SetMethod(string name)
        {
            if (this.CurrentInstance != null)
                this.CurrentMethod = this.CurrentInstance.GetType().GetMethod(name);
        }

        public void ExecuteStrategy(IEnvelope message)
        {
            try
            {
                // retrieve the conversation from the storage if already in progress:
                this.RetreiveSaga();

                // set the message bus on the saga:
                var messageBus = this.m_container.Resolve<IMessageBus>();
                ((ESB.Saga.Saga)this.CurrentInstance).Bus  = messageBus;

                // set the identifier on the saga (if starting a new one):
                this.SetIdentifierForSaga();

                // create or set the message id used for the saga on the 
                // message that is being passed into the saga:
                this.SetSagaIdentifierOnCurrentMessage(message);

                // invoke the method on the endpoint with the current message:
                var dispatcher = this.m_container.Resolve<IMessageDispatcher>();
                dispatcher.Dispatch(CurrentInstance, message);

                // persist the saga to storage (if needed):
                this.PersistSaga(CurrentInstance);

                // tell the endpoint activator that the strategy has completed.
                OnSagaMessageHandlingStrategyCompleted(message);

            }
            catch (Exception exception)
            {
                var msg =
                    string.Format(
                        "An error has occurred while attempting to mediate the conversation for '{0}'. Reason: {1}",
                        this.CurrentInstance.GetType().FullName, exception.ToString());
                throw new Exception(msg, exception);
            }

        }

        /// <summary>
        /// This will set the current conversation identifier on the message if 
        /// the conversation is already in progress. If the conversation is 
        /// initiated by the message, it will start a new conversation with an identifier
        /// and all messages received on the inprogress conversation will receive the 
        /// identifier for proper correlation.
        /// </summary>
        /// <param name="envelope">Message being passed into the conversation.</param>
        private void SetSagaIdentifierOnCurrentMessage(IEnvelope envelope)
        {
            var message = envelope.Body.GetPayload<object>();

            var isNewConversation =
                this.CurrentMethod.GetCustomAttributes(typeof(InitiatedByAttribute), true).Length > 0;

            var isOngoingConversation =
                this.CurrentMethod.GetCustomAttributes(typeof(OrchestratedByAttribute), true).Length > 0;

            if (isNewConversation || isOngoingConversation)
            {
                if (message == null)
                    return;

                ISagaMessage sagaMessage = null;

                try
                {
                    sagaMessage = message as ISagaMessage;
                }
                catch
                {
                    return;
                }

                // throw an exception if a message with a different correlation identifier attempts 
                // to join this conversation instance (cross barrier exception):

                if (((ISaga)this.CurrentInstance).SagaId == Guid.Empty)
                    ((ISaga)this.CurrentInstance).SagaId = sagaMessage.SagaId;

                if (sagaMessage.SagaId != Guid.Empty &&
                    sagaMessage.SagaId != ((ISaga)this.CurrentInstance).SagaId
                    && isOngoingConversation)
                    throw new Exception(string.Format("The current message '{0}' with correlation identifier '{1}' is not part of the existing saga for '{2}' with identifier '{3}'.",
                                                      message.GetType().FullName,
                                                      ((ISagaMessage)message).SagaId.ToString(),
                                                      this.CurrentInstance.GetType().FullName,
                                                      ((ISaga)this.CurrentInstance).SagaId.ToString()));

                sagaMessage.SagaId = ((ISaga)this.CurrentInstance).SagaId;
            }
        }

        /// <summary>
        /// This will retrieve the current state of the conversation (i.e. long-running process) from the 
        /// repository for subsequent processing.
        /// </summary>
        private void RetreiveSaga()
        {
            var reflection = m_container.Resolve<IReflection>();

            if (!typeof(ISaga).IsAssignableFrom(this.CurrentInstance.GetType())) return;

            var sagaId = ((ISaga)this.CurrentInstance).SagaId;

            if (sagaId == Guid.Empty) return;

            var persister = this.FindPersisterForSaga(this.CurrentInstance);
            if (persister == null) return;

            var saga = reflection.InvokeSagaPersisterFind(persister, sagaId);

            if (saga != null)
                CurrentInstance = saga;
        }

        /// <summary>
        /// This will initialize a conversation, if newly started, with an identifier.
        /// </summary>
        private void SetIdentifierForSaga()
        {
            var isNewConversation =
                this.CurrentMethod.GetCustomAttributes(typeof(InitiatedByAttribute), true).Length > 0;

            if (isNewConversation)
                ((ISaga)this.CurrentInstance).SagaId = Guid.NewGuid();
        }

        /// <summary>
        /// This will persist the current state of the conversation (i.e. long-running process) into the 
        /// repository for subsequent processing.
        /// </summary>
        private void PersistSaga(object saga)
        {
            var reflection = m_container.Resolve<IReflection>();

            if (!typeof(ISaga).IsAssignableFrom(saga.GetType())) return;

            var persister = FindPersisterForSaga(saga);

            if (persister == null) return;

            //remove the service bus instance and prepare for storage:
            ((ESB.Saga.Saga) saga).Bus = null;
            var currentSaga = saga as ISaga;
           
            if (!currentSaga.IsCompleted)
                reflection.InvokeSagaPersisterSave(persister, currentSaga);
            else
            {
                reflection.InvokeSagaPersisterComplete(persister, currentSaga.SagaId);
            }
        }

        private object FindPersisterForSaga(object saga)
        {
            object persister = null;

            var reflection = m_container.Resolve<IReflection>();

            // configure the default implementation of persistance, if none found:
            try
            {
                var type = reflection.GetGenericVersionOf(typeof(ISagaPersister<>),
                                                                                  saga.GetType());
                var instance = m_container.Resolve(type);
            }
            catch (Exception exception)
            {
                // no type defined...configure for in-memory persistance:
                m_container.Resolve<IObjectBuilder>().Register(typeof(ISagaPersister<>).Name,
                    typeof(ISagaPersister<>),
                    typeof(InMemorySagaPersister<>), ActivationStyle.AsSingleton);
            }

            // create the instance for persisting sagas, by type:
            try
            {
                var persisterType = reflection.GetGenericVersionOf(typeof(ISagaPersister<>),
                                                                   saga.GetType());
                persister = m_container.Resolve(persisterType);
            }
            catch
            {
                throw;
            }

            return persister;
        }

        /// <summary>
        /// This will invoke the event to let the triggering message endpoint activator 
        /// know that the strategy for handling the message has been completed.
        /// </summary>
        /// <param name="envelope"></param>
        private void OnSagaMessageHandlingStrategyCompleted(IEnvelope envelope)
        {
            EventHandler<MessageHandlingStrategyCompleteEventArgs> evt = this.ChannelStrategyCompleted;
            if (evt != null)
                evt(this, new MessageHandlingStrategyCompleteEventArgs(string.Empty, envelope));
        }
    }
}