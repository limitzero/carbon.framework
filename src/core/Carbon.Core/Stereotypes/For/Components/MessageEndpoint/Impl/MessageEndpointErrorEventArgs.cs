using System;
using System.Text;

namespace Carbon.Core.Stereotypes.For.Components.MessageEndpoint.Impl
{
    public class MessageEndpointActivatorErrorEventArgs : EventArgs
    {
        public object Instance { get; private set; }
        public string MethodName { get; private set; }
        public IEnvelope Envelope { get; private set; }
        public Exception Exception { get; set; }
        public string Message { get; private set; }

        public MessageEndpointActivatorErrorEventArgs(object instance, 
            string methodName, IEnvelope envelope, Exception exception)
        {
            Instance = instance;
            MethodName = methodName;
            Envelope = envelope;
            Exception = exception;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Instance: ").Append(this.Instance.GetType().FullName).Append(Environment.NewLine);
            sb.Append("Method: ").Append(this.MethodName).Append(Environment.NewLine);
            sb.Append("Payload: ").Append(this.Envelope.Body.GetPayload<object>().GetType().FullName).Append(Environment.NewLine);
            sb.Append("Exception: ").Append(this.Exception.ToString()).Append(Environment.NewLine);
            sb.Append("Message: ").Append(this.Message).Append(Environment.NewLine);

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() + this.Message.Length ^ 3;
        }
    }
}