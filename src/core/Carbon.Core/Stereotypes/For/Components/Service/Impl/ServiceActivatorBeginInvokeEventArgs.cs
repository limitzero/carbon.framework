using System;
using Kharbon.Core;

namespace Carbon.Core.Stereotypes.For.Components.Service.Impl
{
    public class ServiceActivatorBeginInvokeEventArgs  : EventArgs
    {
        public object ServiceInstance { get; private set; }
        public string MethodName { get; private set; }
        public IEnvelope Envelope { get; private set; }
        public string Message { get; private set; }

        public ServiceActivatorBeginInvokeEventArgs(object serviceInstance, string methodName, IEnvelope envelope)
        {
            ServiceInstance = serviceInstance;
            MethodName = methodName;
            Envelope = envelope;
            this.Message = string.Format("Begin Invoke: Component '{0}' for method '{1}' with input message '{2}'.",
                                         serviceInstance.GetType().FullName, methodName,
                                         envelope.Body.GetPayload<object>().GetType().FullName);
        }
    }
}