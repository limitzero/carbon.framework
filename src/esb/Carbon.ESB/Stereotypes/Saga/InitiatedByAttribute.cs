using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.ESB.Stereotypes.Saga.Impl;

namespace Carbon.ESB.Stereotypes.Conversations
{
    /// <summary>
    /// Attribute to denote the start of a long-running process:
    /// </summary>
    /// <example>
    ///  
    /// [MessageEndpoint("new_customer_access")]
    /// public class CustomerAccessSaga : Saga
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
            Strategy = typeof (SagaMessageHandlingStrategy);
        }
    }
}