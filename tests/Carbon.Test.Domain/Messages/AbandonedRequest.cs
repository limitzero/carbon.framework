using System;
using Carbon.Core.Stereotypes.For.Components.Message;
using Carbon.ESB.Saga;

namespace Carbon.Test.Domain
{
    [Message]
    public class AbandonedRequest : ISagaMessage
    {
        public string Username { get; set; }
        public string Email { get; set; }

        #region ISagaMessage Members

        public Guid SagaId { get; set; }

        #endregion
    }
}