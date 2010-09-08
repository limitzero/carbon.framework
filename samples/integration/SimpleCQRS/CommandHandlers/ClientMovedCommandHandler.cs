using System;
using Carbon.Core;

namespace SimpleCQRS.CommandHandlers
{

    public class ClientMovedCommandHandler
        : ICanConsume<ClientMovedCommandHandler>
    {
        public void Consume(ClientMovedCommandHandler message)
        {
            throw new NotImplementedException();
        }
    }
}