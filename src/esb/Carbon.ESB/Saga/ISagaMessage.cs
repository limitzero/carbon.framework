using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.ESB.Saga
{
    /// <summary>
    /// Contract for all messages that participate in long-running transactions.
    /// </summary>
    public interface ISagaMessage
    {
        /// <summary>
        /// (Read-Write). The identifier tying all messages to a given saga.
        /// </summary>
        Guid SagaId { get; set; }
    }
}