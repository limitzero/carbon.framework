using System;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Stereotypes.For.Components.MessageEndpoint;
using Carbon.ESB.Messages;

namespace Carbon.ESB.Services.Impl.Timeout
{
    [MessageEndpoint("timeout")]
    public class TimeoutConsumer
    {
        private readonly IMessageBus m_bus;
        
        public TimeoutConsumer(IMessageBus bus)
        {
            m_bus = bus;
        }

        public void RegisterTimeout(TimeoutMessage message)
        {
            var service = m_bus.GetComponent<ITimeoutBackgroundService>();

            if (service == null)
                return;

            try
            {
                var msg = string.Format("Time out registered for message '{0}' created at '{1}' for invocation at '{2}'.",
                                        message.DelayedMessage.GetType().FullName,
                                        message.Created.ToString(),
                                        message.At.ToString());

                m_bus.GetComponent<IAdapterMessagingTemplate>()
                    .DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                            new Envelope(msg));

                service.RegisterTimeout(message);
            }
            catch
            {
            }
        }

        public void RegisterCancellation(CancelTimeoutMessage message)
        {
            var service = m_bus.GetComponent<ITimeoutBackgroundService>();

            if (service == null)
                return;

            try
            {
                m_bus.GetComponent<IAdapterMessagingTemplate>()
                    .DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                            new Envelope(string.Format("Cancellation message registered for message '{0}'.",
                                                       message.Message.GetType().FullName)));

                service.RegisterCancellation(message);
            }
            catch
            {
            }
        }

        public void DeliverExpiredMessage(ExpiredTimeOutMessage message)
        {
            m_bus.Publish(message.Message);

            try
            {
                m_bus.GetComponent<IAdapterMessagingTemplate>()
                    .DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                            new Envelope(string.Format("Delayed message '{0}' delivered.",
                                                       message.Message.GetType().FullName)));
            }
            catch
            {
            }

        }
    }
}