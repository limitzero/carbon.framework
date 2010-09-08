using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Core.Stereotypes.For.MessageHandling;

namespace Carbon.ESB.Stereotypes
{
    interface IServiceBusMessageHandlingStrategy : IMessageHandlingStrategy
    {
        IMessageBus Bus { get; set; }
    }
}
