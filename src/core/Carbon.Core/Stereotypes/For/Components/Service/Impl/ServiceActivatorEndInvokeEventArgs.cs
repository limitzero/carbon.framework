using System;

namespace Carbon.Core.Stereotypes.For.Components.Service.Impl
{
    public class ServiceActivatorEndInvokeEventArgs : EventArgs
    {
        public object ServiceInstance { get; private set; }
        public string MethodName { get; private set; }
        public IEnvelope Envelope { get; private set; }
        public string Message { get; private set; }

        public ServiceActivatorEndInvokeEventArgs(object serviceInstance, string methodName, IEnvelope envelope)
        {
            ServiceInstance = serviceInstance;
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
                                         serviceInstance.GetType().FullName, methodName,
                                         messageType);
        }
    }
}