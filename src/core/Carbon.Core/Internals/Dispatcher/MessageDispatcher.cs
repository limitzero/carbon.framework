using System;
using Carbon.Core;
using Carbon.Core.Internals.MessageResolution;

namespace Carbon.Core.Internals.Dispatcher
{
    public class MessageDispatcher : IMessageDispatcher
    {
        public IEnvelope Dispatch(object instance, IEnvelope message)
        {
            return this.Dispatch(instance, message.Body.GetPayload<object>(), string.Empty);
        }

        public IEnvelope Dispatch(object instance, IEnvelope message, string method)
        {
            return this.Dispatch(instance, message.Body.GetPayload<object>(), method);
        }

        public IEnvelope Dispatch(object instance, object message)
        {
            return this.Dispatch(instance, message, string.Empty);
        }

        public IEnvelope Dispatch(object instance, object message, string method)
        {
            IEnvelope envelope = new NullEnvelope();

            try
            {
                var methodResolver = new MapMessageToMethod();
                var methodToInvoke = methodResolver.Map(instance, message);

                var methodInvoker = new MappedMessageToMethodInvoker(instance, methodToInvoke);
                envelope = methodInvoker.Invoke(message);
            }
            catch (Exception exception)
            {
                throw;
            }

            return envelope;
        }


    }
}