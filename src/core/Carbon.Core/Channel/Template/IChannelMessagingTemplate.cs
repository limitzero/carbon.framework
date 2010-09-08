using Carbon.Core.Templates.Messaging;

namespace Carbon.Core.Channel.Template
{
    /// <summary>
    /// Contract that can be used for channels to send messages back and forth to 
    /// each other.
    /// </summary>
    public interface IChannelMessagingTemplate : IMessagingTemplate<string>
    {
        
    }
}