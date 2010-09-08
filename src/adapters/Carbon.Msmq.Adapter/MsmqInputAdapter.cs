using System;
using System.IO;
using System.Messaging;
using System.Text;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;

namespace Carbon.Msmq.Adapter
{
    /// <summary>
    /// Adapter for taking messages from a MSMQ location and loading them onto a channel.
    /// </summary>
    public class MsmqInputAdapter : AbstractInputChannelAdapter
    {
        private MessageQueue m_message_queue = null;
        private Message m_retrieved_message = null;

        public MsmqInputAdapter(IObjectBuilder builder)
            : base(builder)
        {
            base.IsTransactional = true;
        }

        public override Tuple<IEnvelopeHeader, byte[]> DoReceive()
        {
            var contents = this.ExtractMessageContents();
            var header = this.CreateMessageHeader();
            var tuple = new Tuple<IEnvelopeHeader, byte[]>(header, contents);
            return tuple;
        }

        public override byte[] ExtractMessageContents()
        {
            byte[] retval = null;

            this.SetLocalQueue(this.Uri);

            if (TryPeek())
            {
                try
                {
                    m_retrieved_message = m_message_queue.Receive(TimeSpan.FromSeconds(2));
                                                                  //MsmqAdapterUtils.GetTransactionTypeForSend(this.IsTransactional));

                    if (m_retrieved_message != null)
                    {

                        using (TextReader reader = new StreamReader(this.m_retrieved_message.BodyStream))
                        {
                            var enc = new UTF8Encoding();
                            retval = enc.GetBytes(reader.ReadToEnd());
                        }
                    }

                }
                catch (MessageQueueException mex)
                {
                    // nothing to do here...wait for the next message.
                }
                catch (Exception exception)
                {
                }
            }

            return retval;
        }

        public override IEnvelopeHeader CreateMessageHeader()
        {
            IEnvelopeHeader header = new EnvelopeHeader();

            if (this.m_retrieved_message == null) return header;

            header.SetMessageId(this.m_retrieved_message.Id);

            if (!string.IsNullOrEmpty(this.m_retrieved_message.CorrelationId))
                header.CorrelationId = this.m_retrieved_message.CorrelationId;

            return header;
        }

        private IEnvelope ReceiveMessage(string location)
        {
            IEnvelope message = new NullEnvelope();

            this.SetLocalQueue(location);

            if (TryPeek())
            {
                try
                {
                    var receivedMessage = m_message_queue.Receive(TimeSpan.FromSeconds(2),
                                                                  MsmqAdapterUtils.GetTransactionTypeForSend(this.IsTransactional));
                    if (receivedMessage != null)
                        message = CreateMessageForMSMQMessage(receivedMessage);
                }
                catch (MessageQueueException mex)
                {
                    // nothing to do here...wait for the next message.
                }
                catch (Exception exception)
                {
                }

            }

            return message;
        }

        private void SetLocalQueue(string queue)
        {
            var path = MsmqAdapterUtils.RetreiveLocationFromProtocolUri(queue);

            try
            {
                MsmqAdapterUtils.CreateTransactonalQueue(path);
            }
            catch
            {

            }

            m_message_queue = new MessageQueue(path);

            var mpf = new MessagePropertyFilter();

            try
            {
                mpf.SetAll();
            }
            catch
            {

            }

            m_message_queue.MessageReadPropertyFilter = mpf;
        }

        private bool TryPeek()
        {
            var needToHandle = false;

            try
            {
                m_message_queue.Peek(TimeSpan.FromSeconds(1));
                needToHandle = true;
            }
            catch (MessageQueueException mex)
            {

            }
            catch (Exception exception)
            {

            }

            return needToHandle;
        }

        private IEnvelope CreateMessageForMSMQMessage(System.Messaging.Message queueMessage)
        {
            IEnvelope message = new Envelope();

            var contents = string.Empty;
            byte[] payload = { };

            try
            {
                using (TextReader reader = new StreamReader(queueMessage.BodyStream))
                {
                    contents = reader.ReadToEnd();
                    payload = ASCIIEncoding.ASCII.GetBytes(contents);
                }

                message.Body.SetPayload(payload);
            }
            catch (Exception exception)
            {
                // just return the string instance of the message:
                message.Body.SetPayload(payload);
            }

            message.Header.SetMessageId(queueMessage.Id);

            if (!string.IsNullOrEmpty(queueMessage.CorrelationId))
                message.Header.CorrelationId = queueMessage.CorrelationId;

            return message;
        }

    }
}