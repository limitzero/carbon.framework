using System;
using System.Reflection;
using Carbon.Core;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;

namespace Carbon.Integration.Scheduler
{
    public class MethodInvokerScheduledTask : IMethodInvokerScheduledTask
    {
        public event EventHandler<ScheduledTaskExecutedEventArgs> ScheduledTaskExecuted;
        public event EventHandler<ScheduledTaskErrorEventArgs> ScheduledTaskError;

        public object Instance { get; set; }
        public MethodInfo Method { get; set; }
        public object[] Messages { get; set; }
        public int Frequency { get; set; }

        public void Execute()
        {
            try
            {
                var result = Method.Invoke(Instance, Messages);
                var message = BuildMessage(result, Instance);
                OnScheduledTaskExecuted(message);
            }
            catch (Exception exception)
            {
                if(!OnScheduledTaskError(this.Instance.GetType().Name, this.Method.Name, exception))
                    throw;
            }
           
        }

        private IEnvelope BuildMessage(object result, object messagingEndpoint)
        {
            var channel = ExtractChannelFromMessageEndpoint(messagingEndpoint);
            var message = new Envelope(result);
            message.Header.OutputChannel = channel;
            return message;
        }

        /// <summary>
        /// This will extract the output channel name from a messaging end point.
        /// </summary>
        /// <param name="messagingEndpoint">Instance of the messaging end point.</param>
        /// <returns></returns>
        private string ExtractChannelFromMessageEndpoint(object messagingEndpoint)
        {
            var channelName = string.Empty;

            var attrs = messagingEndpoint.GetType().GetCustomAttributes(typeof(MessageEndpointAttribute), true);
            if (attrs.Length > 0)
            {
                channelName = ((MessageEndpointAttribute)attrs[0]).OutputChannel;
            }

            return channelName;
        }

        private bool OnScheduledTaskError(string instanceName, string methodName, Exception exception)
        {
            
            EventHandler<ScheduledTaskErrorEventArgs> evt = this.ScheduledTaskError;
            var isEventHandlerAttached = (evt != null);

            if(isEventHandlerAttached)
                evt(this, new ScheduledTaskErrorEventArgs(instanceName, methodName, exception));

            return isEventHandlerAttached;
        }

        private void OnScheduledTaskExecuted(IEnvelope message)
        {
            EventHandler<ScheduledTaskExecutedEventArgs> evt = this.ScheduledTaskExecuted;
            if (evt != null)
                evt(this, new ScheduledTaskExecutedEventArgs(message));
        }
    }
}