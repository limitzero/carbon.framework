using System;
using System.Reflection;
using Carbon.Channel.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Channel;
using Carbon.Core.Channel.Impl.Null;
using Carbon.Core.Internals.Reflection;
using Kharbon.Core;
using Kharbon.Stereotypes.For.MessageHandling.Conversations;

namespace Carbon.Core.Stereotypes.For.MessageHandling.Conversations.Impl
{
    /// <summary>
    /// Concrete strategy for handling long-running conversations for a message endpoint.
    /// </summary>
    public class ConversationMessageHandlingStrategy : IMessageHandlingStrategy
    {
        private IObjectBuilder m_container = null;

        public event EventHandler<MessageHandlingStrategyCompleteEventArgs> ChannelStrategyCompleted;
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
                this.RetreiveConversation();

                // set the context on the conversation:
                var messageBus = this.m_container.Resolve<IMessageBus>();
                ((IConversation)this.CurrentInstance).Bus = messageBus;

                // set the identifier on the conversation (if starting a new one):
                this.SetIdentifierForConversation();

                // create or set the message id used for the conversation on the 
                // message that is being passed into the conversation:
                this.SetConversationIdentifierOnCurrentMessage(message);

                // invoke the method on the endpoint with the current message:
                var dispatcher = this.m_container.Resolve<IMessageDispatcher>();
                dispatcher.Dispatch(this.CurrentInstance, message);

                // persist the conversation to storage (if needed):
                this.PersistConversation(this.CurrentInstance);

                // tell the endpoint activator that the strategy has completed.
                OnConversationStrategyCompleted(message);

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
        private void SetConversationIdentifierOnCurrentMessage(IEnvelope envelope)
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

                IConversationMessage conversationMessage = null;

                try
                {
                    conversationMessage = message as IConversationMessage;
                }
                catch
                {
                    return;
                }

                // throw an exception if a message with a different correlation identifier attempts 
                // to join this conversation instance (cross barrier exception):

                if (((IConversation)this.CurrentInstance).Id == Guid.Empty)
                    ((IConversation) this.CurrentInstance).Id = conversationMessage.CorrelationId;

                if (conversationMessage.CorrelationId != Guid.Empty &&
                    conversationMessage.CorrelationId != ((IConversation)this.CurrentInstance).Id
                    && isOngoingConversation)
                    throw new Exception(string.Format("The current message '{0}' with correlation identifier '{1}' is not part of the existing conversation for '{2}' with identifier '{3}'.",
                                                      message.GetType().FullName,
                                                      ((IConversationMessage)message).CorrelationId.ToString(),
                                                      this.CurrentInstance.GetType().FullName,
                                                      ((IConversation)this.CurrentInstance).Id.ToString()));

                conversationMessage.CorrelationId = ((IConversation)this.CurrentInstance).Id;

            }
        }

        /// <summary>
        /// This will retrieve the current state of the conversation (i.e. long-running process) from the 
        /// repository for subsequent processing.
        /// </summary>
        private void RetreiveConversation()
        {
            var reflection = m_container.Resolve<IReflection>(); 

            if (!typeof(IConversation).IsAssignableFrom(this.CurrentInstance.GetType())) return;

            var conversationId = ((IConversation) this.CurrentInstance).Id;

            var persister = this.FindPersisterForConversation(this.CurrentInstance);
            if(persister == null) return;

            var conversation = reflection.InvokeConversationPersisterFind(persister, conversationId);
           
            if (conversation != null)
                // check to see if the conversation should be continued:
                if (!((IConversation)conversation).IsCompleted)
                    this.CurrentInstance = conversation;
        }

        /// <summary>
        /// This will initialize a conversation, if newly started, with an identifier.
        /// </summary>
        private void SetIdentifierForConversation()
        {
            var isNewConversation =
                this.CurrentMethod.GetCustomAttributes(typeof(InitiatedByAttribute), true).Length > 0;

            if (isNewConversation)
                ((IConversation)this.CurrentInstance).Id = Guid.NewGuid();
        }

        /// <summary>
        /// This will persist the current state of the conversation (i.e. long-running process) into the 
        /// repository for subsequent processing.
        /// </summary>
        private void PersistConversation(object conversation)
        {
            var reflection = m_container.Resolve<IReflection>();

            if (typeof(IConversation).IsAssignableFrom(conversation.GetType()))
            {

                var conversationId =((IConversation) conversation).Id;

                var persister = this.FindPersisterForConversation(conversation);

                if(persister == null) return; 

                var cnv = conversation as IConversation;

                if (cnv != null)
                {
                    if (cnv.IsCompleted)
                        reflection.InvokeConversationPersisterComplete(persister, conversationId);
                    else
                    {
                        reflection.InvokeConversationPersisterSave(persister, conversation);
                    }
                }

            }

        }

        private object FindPersisterForConversation(object conversation)
        {
            object persister = null;

            var reflection = m_container.Resolve<IReflection>();

            try
            {
                var persisterType = reflection.GetGenericVersionOf(typeof(IConversationPersister<>),
                                                                   conversation.GetType());

                persister = m_container.Resolve(persisterType);

            }
            catch
            {
               
            }

            return persister;
        }

        /// <summary>
        /// This will invoke the event to let the triggering message endpoint activator 
        /// know that the strategy for handling the message has been completed.
        /// </summary>
        /// <param name="envelope"></param>
        private void OnConversationStrategyCompleted(IEnvelope envelope)
        {
            EventHandler<MessageHandlingStrategyCompleteEventArgs> evt = this.ChannelStrategyCompleted;
            if (evt != null)
                evt(this, new MessageHandlingStrategyCompleteEventArgs(string.Empty, envelope));
        }
    }
}