using System;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB;
using Carbon.Test.Domain.PingPongMessages;

namespace Carbon.Test.Domain
{
    [MessageEndpoint("ping")]
    public class PingConsumer
    {
        private readonly IMessageBus m_bus;

        public PingConsumer(IMessageBus bus)
        {
            m_bus = bus;
        }

        public void Consume(PingMessage message)
        {
            Console.WriteLine("Received ping message...");
        }

    }
}