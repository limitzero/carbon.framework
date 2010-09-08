using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.ESB.Services
{
    /// <summary>
    /// Contract for services that will process control messages for the message bus.
    /// </summary>
    public interface IMessageBusService
    {
        /// <summary>
        /// (Read-Write). Instance of the message bus for sending control messages.
        /// </summary>
        IMessageBus Bus { get; set; }
    }
}