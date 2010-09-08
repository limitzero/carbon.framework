using System;
using Carbon.Core.Stereotypes.For.Components.Message;
using Carbon.ESB.Saga;

namespace Carbon.Test.Domain.Messages
{
    [Message]
    public class RequestReceived : ISagaMessage
    {
        public string Username { get; set; }
        public string Email { get; set; }

        #region ISagaMessage Members

        public Guid SagaId { get; set; }

        #endregion
    }
}