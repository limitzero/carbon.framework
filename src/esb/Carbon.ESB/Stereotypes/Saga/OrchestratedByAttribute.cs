using System;
using Carbon.Core.Stereotypes.For.MessageHandling;
using Carbon.ESB.Stereotypes.Saga.Impl;

namespace Carbon.ESB.Stereotypes.Saga
{
    /// <summary>
    /// Attribute to denote the an intermediary point of a long-running process:
    /// </summary>
    /// <example>
    ///  
    /// [MessageEndpoint("new_customer_access")]
    /// public class CustomerAccessSaga : Saga
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
            Strategy = typeof(SagaMessageHandlingStrategy);
        }
    }
}