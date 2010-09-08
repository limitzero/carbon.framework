using System;

namespace Carbon.Core.Stereotypes.For.MessageHandling
{
    /// <summary>
    /// Contract that delineates that special behavior should be applied to the message 
    /// while being processed over the channel to the message endpoint.
    /// </summary>
    public interface IMessageHandlingStrategyAttribute
    {
        /// <summary>
        /// (Read-Write). The type corresponding to the component that will implement the strategy 
        /// for handling the message that is different from the message endpoint component.
        /// </summary>
        Type Strategy { get; set; }
    }
}