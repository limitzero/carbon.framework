using System;
using System.Linq;
using Carbon.Core;
using Carbon.Core.Adapter.Template;
using Carbon.ESB.Messages;
using Carbon.ESB.Services.Impl.Timeout.Persister;

namespace Carbon.ESB.Services.Impl.Timeout
{
    /// <summary>
    /// Mesage bus background service to poll for timeouts.
    /// </summary>
    public class TimeoutBackgroundService :
        ContextBackgroundService, ITimeoutBackgroundService
    {
        private readonly IMessageBus m_bus;
        private readonly ITimeoutsPersister m_persister;

        public TimeoutBackgroundService(IMessageBus bus, ITimeoutsPersister persister)
        {
            m_bus = bus;
            m_persister = persister;
            this.Name = "Timeout";
        }

        public override void PerformAction()
        {
            var timeouts = m_persister.FindAllExpiredTimeouts();

            if (timeouts.Count() == 0) return;

            foreach (var message in timeouts)
            {
                m_bus.Publish(message.DelayedMessage);

                m_bus.GetComponent<IAdapterMessagingTemplate>()
                    .DoSend(new Uri(Constants.LogUris.DEBUG_LOG_URI),
                            new Envelope(string.Format("Delayed message '{0}' delivered.",
                                                       message.DelayedMessage.GetType().FullName)));

                m_persister.Complete(message);
            }

        }

        public void RegisterTimeout(TimeoutMessage message)
        {
            m_persister.Save(message);
        }

        public void RegisterCancellation(CancelTimeoutMessage message)
        {
            m_persister.AbortTimeout(message);
        }

    }
}