using System;
using System.Collections.Generic;
using Carbon.Core.Builder;

namespace Carbon.Core.Pipeline.Component
{
    /// <summary>
    /// Default pipeline component that will eliminate duplicate messages from being sent or received to/from a location.
    /// </summary>
    public class NonIdempotentPipelineComponent : AbstractPipelineComponent
    {
        private static bool m_is_busy;
        private static List<object> m_message_payloads = null;

        /// <summary>
        /// (Read-Write). The number of unique messages to keep in the local cache before auto-refresh (default is 500).
        /// </summary>
        public int CachedMessagesLimit { get; set; }

        public NonIdempotentPipelineComponent(IObjectBuilder objectBuilder) : base(objectBuilder)
        {
            Name = "Non-Idempontent (No-Dups) Message Pipeline Component";
            if(m_message_payloads == null)
                m_message_payloads = new List<object>();
        }

        public override IEnvelope Execute(IEnvelope envelope)
        {
            if (CachedMessagesLimit == 0) CachedMessagesLimit = 500;

            try
            {
                OnComponentStarted(this, envelope);

                var payload = envelope.Body.GetPayload<object>();

                while(m_is_busy)
                {
                    System.Threading.Thread.Sleep(100);
                }

                if(m_message_payloads.Contains(payload))
                    envelope = new NullEnvelope();
                else
                {
                    lock(m_message_payloads)
                        m_message_payloads.Add(payload);
                } 

                if(m_message_payloads.Count == CachedMessagesLimit)
                {
                    m_is_busy = true; 

                    lock(m_message_payloads)
                        m_message_payloads.Clear();
                }

                OnComponentCompleted(this, envelope);
            }
            catch (Exception exception)
            {
                if (!OnComponentError(this, envelope, exception))
                    throw;
            }

            return envelope;
        }
       
    }


}
