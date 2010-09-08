using System;
using System.Collections.Generic;

namespace Carbon.Core
{
    public class EnvelopeHeader : IEnvelopeHeader
    {
        private List<EnvelopeHeaderItem> m_items = null;

        /// <summary>
        /// (Read-Only). The series of name-value items for the message.
        /// </summary>
        public EnvelopeHeaderItem[] HeaderItems { get; private set; }

        /// <summary>
        /// (Read-Only). The name of the channel that has received the message.
        /// </summary>
        public string InputChannel { get; set; }

        /// <summary>
        /// (Read-Write). The name of the channel where the processed message will be sent.
        /// </summary>
        public string OutputChannel { get; set; }

        /// <summary>
        /// (Read-Only). The unique identifier for this message.
        /// </summary>
        public string MessageId { get; private set; }

        /// <summary>
        /// (Read-Write). The unique identification for the message or stream 
        /// of messages for a logical processing context.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// (Read-Write). The transport location where the message will be delivered.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// (Read-Write). The channel that is set by the requestor it which it 
        /// will expect the reply to be generated for the request.
        /// </summary>
        public string ReturnAddress { get; set; }


        public DateTime ExpiresOn { get; set; }

        /// <summary>
        /// (Read-Write). The current instance of a message within a stream of messages uniquely tied by correlation 
        /// identifier.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// (Read-Write). The current size of the stream correlated messages.
        /// </summary>
        public int SequenceSize { get; set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public EnvelopeHeader()
        {
            this.MessageId = string.Format("{0}-{1}", "MSG", System.Guid.NewGuid().ToString());
            this.m_items = new List<EnvelopeHeaderItem>();
        }

        /// <summary>
        /// This will allow for the message id to be manually set on the envelope.
        /// </summary>
        /// <param name="messageId"></param>
        public void SetMessageId(string messageId)
        {
            this.MessageId = messageId;
        }


        /// <summary>
        /// This will allow for a custom name-value pair to be stored in the header of the message.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeaderItem(string name, object value)
        {
            var item = new EnvelopeHeaderItem(name, value);

            if (!m_items.Contains(item))
            {
                m_items.Add(item);
                this.HeaderItems = this.m_items.ToArray();
            }
        }

        /// <summary>
        /// This will extract the value for a header stored on the envelope.
        /// </summary>
        /// <param name="name">Key for the header value.</param>
        /// <returns></returns>
        public IEnvelopeHeaderItem GetHeaderItem(string name)
        {
            IEnvelopeHeaderItem item = new NullEnvelopeHeaderItem();

            if(this.HeaderItems == null) return item;

            foreach (var itm in HeaderItems)
            {
                if (itm.Name.ToLower().Trim() != name.Trim().ToLower()) continue;
                item = itm;
                break;
            }

            return item;
        }

        /// <summary>
        /// This will allow for the headers of a message to be copied into a new envelope.
        /// </summary>
        /// <param name="headers"></param>
        public void CopyHeaders(IEnumerable<EnvelopeHeaderItem> headers)
        {
            this.m_items = new List<EnvelopeHeaderItem>(headers);
            this.HeaderItems = this.m_items.ToArray();
        }
    }
}