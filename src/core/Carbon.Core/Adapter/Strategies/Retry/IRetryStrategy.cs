using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Core.Adapter.Strategies.Retry
{
    /// <summary>
    /// Contract for output adapters to submit a message to a physical location.
    /// </summary>
    public interface IRetryStrategy
    {
        /// <summary>
        /// (Read-Write). The number of attempts to try and deliver the message to the location 
        /// after the first error.
        /// </summary>
        int MaxRetries { get; set; }

        /// <summary>
        /// (Read-Write). The number of seconds to wait in between each retry of the message
        /// delivery.
        /// </summary>
        int WaitInterval { get; set; }

        /// <summary>
        /// (Read-Write). The uri to send the message to if it has failed on retry.
        /// </summary>
        string FailureDeliveryUri { get; set; }
    }
}