using System;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.Test.Domain.PingPongMessages;

namespace Carbon.Test.Domain
{
    [MessageEndpoint("pong")]
    public class PongConsumer 
    {
        public void Consume(PongMessage message)
        {
            Console.WriteLine("Received pong message...");
        }
    }
}