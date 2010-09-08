using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Core.Stereotypes.For.MessageHandling
{
    /// <summary>
    /// Attribute to denote a subscribing method that will accept all messages 
    /// that can be assigned to that type.
    /// </summary>
    /// <example>
    ///  
    /// [MessageEndpoint("new_customer_access")]
    /// public class TradeConversation
    /// {
    ///     public void Process([MatchAll] ITradeUpdated tradeUpdated)
    ///     {
    ///        // this will listen for all messages that are derived from ITradeUpdated.
    ///     }
    /// }
    /// 
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class MatchAllAttribute : Attribute
    {
    }
}