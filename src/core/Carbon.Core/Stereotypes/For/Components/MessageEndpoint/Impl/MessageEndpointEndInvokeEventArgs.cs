using System;

namespace Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl
{
    public class MessageEndpointActivatorEndInvokeEventArgs : EventArgs
    {
        public object Instance { get; private set; }
        public string MethodName { get; private set; }
        public IEnvelope Envelope { get; private set; }
        public string Message { get; private set; }

        public MessageEndpointActivatorEndInvokeEventArgs(object instance, string methodName, IEnvelope envelope)
        {
            Instance = instance;
            MethodName = methodName;
            Envelope = envelope;

            var messageType = string.Empty;

            if(envelope is NullEnvelope)
                messageType = "<Null>";
            else
            {
                messageType = envelope.Body.GetPayload<object>().GetType().FullName;
            }

            this.Message = string.Format("End Invoke: Component '{0}' for method '{1}' with output message '{2}'.",
                                         instance.GetType().FullName, methodName,
                                         messageType);
        }
    }
}