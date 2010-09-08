using System;
using System.Collections.Generic;

namespace Carbon.Core
{
    public interface IEnvelopeHeader
    {
        /// <summary>
        /// (Read-Only). The unique identifier for this message.
        /// </summary>
        string MessageId { get; }

        /// <summary>
        /// (Read-Write). The unique identification for the message or stream 
        /// of messages for a logical processing context.
        /// </summary>
        string CorrelationId { get; set; }

        /// <summary>
        /// (Read-Write). The transport location where the message will be delivered.
        /// </summary>
        string Destination { get; set; }

        /// <summary>
        /// (Read-Write). The channel that is set by the requestor it which it 
        /// will expect the reply to be generated for the request.
        /// </summary>
        string ReturnAddress { get; set; }

        DateTime ExpiresOn { get; set; }

        /// <summary>
        /// (Read-Write). The current instance of a message within a stream of messages uniquely tied by correlation 
        /// identifier.
        /// </summary>
        int SequenceNumber { get; set; }

        /// <summary>
        /// (Read-Write). The current size of the stream correlated messages.
        /// </summary>
        int SequenceSize { get; set; }

        /// <summary>
        /// (Read-Only). The series of name-value items for the message.
        /// </summary>
        EnvelopeHeaderItem[] HeaderItems { get; }

        /// <summary>
        /// (Read-Only). The name of the channel that has received the message.
        /// </summary>
        string InputChannel { set; get; }

        /// <summary>
        /// (Read-Write). The name of the channel where the processed message will be sent.
        /// </summary>
        string OutputChannel { set; get; }
        
        /// <summary>
        /// This will allow for the message id to be manually set on the envelope.
        /// </summary>
        /// <param name="messageId"></param>
        void SetMessageId(string messageId);

        /// <summary>
        /// This will allow for a custom name-value pair to be stored in the header of the message.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddHeaderItem(string name, object value);

        /// <summary>
        /// This will allow for the headers of a message to be copied into a new envelope.
        /// </summary>
        /// <param name="headers"></param>
        void CopyHeaders(IEnumerable<EnvelopeHeaderItem> headers);

        /// <summary>
        /// This will extract the value for a header stored on the envelope.
        /// </summary>
        /// <param name="name">Key for the header value.</param>
        /// <returns></returns>
        IEnvelopeHeaderItem GetHeaderItem(string name);
    }
}