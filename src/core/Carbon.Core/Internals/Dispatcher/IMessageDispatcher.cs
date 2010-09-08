namespace Carbon.Core.Internals.Dispatcher
{
    public interface IMessageDispatcher
    {
        IEnvelope Dispatch(object instance, object message);
        IEnvelope Dispatch(object instance, object message, string method);

        IEnvelope Dispatch(object instance, IEnvelope message);
        IEnvelope Dispatch(object instance, IEnvelope message, string method);
    }
}