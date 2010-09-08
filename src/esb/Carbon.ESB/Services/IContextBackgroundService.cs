namespace Carbon.ESB.Services
{
    public interface IContextBackgroundService
    {
        /// <summary>
        /// (Read-Write). The current message bus instance used for mediating the 
        /// messages used in the conversation between endpoints.
        /// </summary>
        IMessageBus Bus { get; }
    }
}