using System;
using System.IO;
using System.Messaging;
using System.Text;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;
using Carbon.Core.Internals;

namespace Carbon.Msmq.Adapter
{
    /// <summary>
    /// Adapter for taking information from a channel and loading it to a MSMQ location.
    /// </summary>
    public class MsmqOutputAdapter : AbstractOutputChannelAdapter
    {
        public MsmqOutputAdapter(IObjectBuilder builder) : base(builder)
        {
            base.IsTransactional = true;
        }

        public override void DoSend(IEnvelope envelope)
        {
            this.SendMessage(envelope);
        }

        private void SendMessage(IEnvelope message)
        {
            try
            {
                SubmitMessage(this.Uri, message);
            }
            catch (Exception ex)
            {
                Utils.Retry(base.ObjectBuilder, (arg1, arg2) => this.SubmitMessage(this.Uri, message),
                    this.Uri, message, base.RetryStrategy);
            }
        }

        private void SubmitMessage(string destination, IEnvelope message)
        {
            var location = MsmqAdapterUtils.RetreiveLocationFromProtocolUri(destination);

            MsmqAdapterUtils.CreateTransactonalQueue(location);

            var toSend = new System.Messaging.Message();

            // message payload should be in byte[] format for delivery:
            var payload = message.Body.GetPayload<byte[]>();

            using (var stream = new MemoryStream(payload))
            using (var queue = new MessageQueue(location, QueueAccessMode.Send))
            {
                stream.Seek(0, SeekOrigin.Begin);
                toSend.BodyStream = stream;
                toSend.Recoverable = true;

                toSend.Label = System.Guid.NewGuid().ToString();

                if (!string.IsNullOrEmpty(message.Header.CorrelationId))
                {
                    toSend.Label = string.Concat("MSG-{", message.Header.CorrelationId, "}");

                    try
                    {
                        toSend.CorrelationId = message.Header.CorrelationId;
                    }
                    catch (Exception exception)
                    {

                    }

                }

                try
                {
                    new TransactionContext(
                        () => queue.Send(toSend, MsmqAdapterUtils.GetTransactionTypeForSend(this.IsTransactional)), this.IsTransactional);
                }
                catch (Exception exception)
                {
                    throw;
                }
            }

        }
    }
}