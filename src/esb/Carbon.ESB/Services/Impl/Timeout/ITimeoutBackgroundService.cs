using Carbon.ESB.Messages;

namespace Carbon.ESB.Services.Impl.Timeout
{
    public interface ITimeoutBackgroundService
    {
        void RegisterTimeout(TimeoutMessage message);
        void RegisterCancellation(CancelTimeoutMessage message);
    }
}