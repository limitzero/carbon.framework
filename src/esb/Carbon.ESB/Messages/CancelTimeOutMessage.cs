using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace Carbon.ESB.Messages
{
    [Message]
    public class CancelTimeoutMessage
    {
        public object Message { get; set; }

        /// <summary>
        /// default .ctor for serialization engine.
        /// </summary>
        public CancelTimeoutMessage()
            :this(null)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="message"></param>
        public CancelTimeoutMessage(object message)
        {
            Message = message;
        }
    }
}