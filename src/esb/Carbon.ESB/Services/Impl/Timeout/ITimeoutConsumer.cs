using Carbon.ESB.Messages;

namespace Carbon.ESB.Services.Impl.Timeout
{
    /// <summary>
    /// Contract for message bus service that will look for and process 
    /// messages related to timeouts.
    /// </summary>
    public interface ITimeoutConsumer //: IMessageBusService
    {
        void RegisterTimeout(TimeoutMessage message);
        void RegisterCancellation(CancelTimeoutMessage message);
        void DeliverExpiredMessage(ExpiredTimeOutMessage message);
    }
}