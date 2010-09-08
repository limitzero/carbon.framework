using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.ESB.Messages;
using Carbon.ESB.Saga;

namespace Carbon.ESB.Services.Impl.Timeout.Persister
{
    /// <summary>
    /// In-memory representation of repository for storing timeouts.
    /// </summary>
    public class InMemoryTimeoutsPersister : ITimeoutsPersister
    {
        private static bool m_is_busy = false;
        private static object m_timeouts_lock = new object();
        private static List<TimeoutMessage> m_timeouts = null;

        public InMemoryTimeoutsPersister()
        {
            if (m_timeouts == null)
                m_timeouts = new List<TimeoutMessage>();
        }

        public TimeoutMessage[] FindAllExpiredTimeouts()
        {
            var messages = m_timeouts.FindAll(x => x.HasExpired());
            return messages.ToArray();
        }

        public void AbortTimeout(CancelTimeoutMessage message)
        {
            lock (m_timeouts_lock)
            {
                m_is_busy = true;

                try
                {
                    // find the set of timeouts that pertain to the 
                    // current message inside of the cancellation 
                    // request (i.e. only cancel the timeouts containing 
                    // the specific delayed messages that we need...not everything):
                    var currentTimeOutMessagesForCancellation =
                        from msg in m_timeouts
                        where msg.DelayedMessage.GetType() == message.Message.GetType()
                        select msg;

                    var messagesToRemove = new List<TimeoutMessage>();

                    // check for messages that are part of a saga as well:
                    if (typeof(ISagaMessage).IsAssignableFrom(message.Message.GetType()))
                    {
                        var conversationTimeOutMessages =
                            from msg in currentTimeOutMessagesForCancellation
                            where
                                typeof(ISagaMessage).IsAssignableFrom(msg.DelayedMessage.GetType())
                            select msg;

                        // must remove the saga messages from the list of time out messages
                        // if the set of conversation identifiers match:
                        foreach (var conversationMsg in conversationTimeOutMessages)
                            foreach (var timeoutMsg in m_timeouts)
                            {
                                if (timeoutMsg.DelayedMessage.GetType() == conversationMsg.DelayedMessage.GetType())
                                    if (typeof(ISagaMessage).IsAssignableFrom(timeoutMsg.DelayedMessage.GetType()))
                                        if (((ISagaMessage)timeoutMsg.DelayedMessage).SagaId ==
                                            ((ISagaMessage)conversationMsg.DelayedMessage).SagaId)
                                            messagesToRemove.Add(timeoutMsg);
                            }

                    }
                    else
                    {
                        // no saga messages present...just add the set of messages retreived.
                        messagesToRemove.AddRange(currentTimeOutMessagesForCancellation);
                    }

                    if (messagesToRemove.Count() == 0)
                        return;

                    var new_time_out_messages =
                        new List<TimeoutMessage>(m_timeouts.AsEnumerable());

                    foreach (var messageToRemove in messagesToRemove)
                    {
                        new_time_out_messages.Remove(messageToRemove);
                    }

                    m_timeouts = new List<TimeoutMessage>(new_time_out_messages);
                }
                catch (Exception exception)
                {
                }

                m_is_busy = false;
            }
        }

        public void Save(TimeoutMessage message)
        {
            lock (m_timeouts_lock)
            {
                while (m_is_busy)
                {
                }

                if (!m_timeouts.Contains(message))
                    m_timeouts.Add(message);
            }
        }

        public void Complete(TimeoutMessage message)
        {
            lock (m_timeouts_lock)
                m_timeouts.Remove(message);
        }
    }
}