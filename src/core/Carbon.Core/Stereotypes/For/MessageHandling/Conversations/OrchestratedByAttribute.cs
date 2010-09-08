using System;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Core.Stereotypes.For.MessageHandling.Conversations.Impl;

namespace Kharbon.Stereotypes.For.MessageHandling.Conversations
{
    /// <summary>
    /// Attribute to denote the an intermediary point of a long-running process:
    /// </summary>
    /// <example>
    ///  
    /// [MessageEndpoint("new_customer_access")]
    /// public class CustomerAccessConversation : Conversation{NullConversationState}
    /// {
    ///     [InitiatedBy]
    ///     public void ProcessLoginRequest(LoginRequest message)
    ///     {}
    ///    
    ///     [OrchestratedBy]
    ///     public void ProcessLoginConfirmation(LoginConfirmation message)
    ///     {}
    /// }
    /// 
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class OrchestratedByAttribute : Attribute, IMessageHandlingStrategyAttribute
    {
        public Type Strategy { get; set; }

        public OrchestratedByAttribute()
        {
            Strategy = typeof(ConversationMessageHandlingStrategy);
        }
    }
}