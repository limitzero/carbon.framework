using System;

namespace Carbon.Core.Stereotypes.For.MessageHandling
{
    public class MessageHandlingStrategyCompleteEventArgs : EventArgs
    {
        public string NextChannel { get; private set; }
        public IEnvelope Message { get; private set; }

        public MessageHandlingStrategyCompleteEventArgs(string nextChannel, IEnvelope message)
        {
            NextChannel = nextChannel;
            message.Header.InputChannel = nextChannel;
            Message = message;
        }
    }
}