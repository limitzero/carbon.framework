using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.Core.Stereotypes.For.MessageHandling.Conversations.Impl;

namespace Kharbon.Stereotypes.For.MessageHandling.Conversations
{
    /// <summary>
    /// Attribute to denote the start of a long-running process:
    /// </summary>
    /// <example>
    ///  
    /// [MessageEndpoint("new_customer_access")]
    /// public class CustomerAccessConversation : Conversation
    /// {
    ///     [InitiatedBy]
    ///     public void ProcessLoginRequest(LoginRequest message)
    ///     {
    ///     }
    /// }
    /// 
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InitiatedByAttribute : Attribute, IMessageHandlingStrategyAttribute
    {
        public Type Strategy { get; set;}

        public InitiatedByAttribute()
        {
            Strategy = typeof (ConversationMessageHandlingStrategy);
        }
    }
}
