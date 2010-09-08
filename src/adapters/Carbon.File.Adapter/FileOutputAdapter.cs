using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;
using Carbon.File.Adapter.Strategies;

namespace Carbon.File.Adapter
{
    /// <summary>
    /// Adapter for taking information from a channel and loading it to a file location.
    /// </summary>
    public class FileOutputAdapter : AbstractOutputChannelAdapter
    {
        public FileOutputAdapter(IObjectBuilder builder) : base(builder)
        {
        }

        public IFileNameStrategy FileNameStrategy { get; set; }

        public override void DoSend(IEnvelope envelope)
        {
            SendMessage(envelope);
        }

        private void SendMessage(IEnvelope message)
        {
            try
            {
                SubmitMessage(this.Uri, message);
            }
            catch 
            {
                Utils.Retry(base.ObjectBuilder, (arg1, arg2) => this.SubmitMessage(this.Uri, message),
                    this.Uri, message, base.RetryStrategy);
            }
        }

        private void SubmitMessage(string destination, IEnvelope message)
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            var file = CreateFileForDelivery(destination, message);

            var payload = new byte[] { };

            if (message.Body.Payload.GetType() == typeof(string))
                payload = ASCIIEncoding.ASCII.GetBytes(message.Body.Payload as string);
            else if (message.Body.Payload.GetType() == typeof(byte[]))
            {
                payload = message.Body.GetPayload<byte[]>(); 
            }
            else
            {
                payload = ASCIIEncoding.ASCII.GetBytes(message.Body.Payload.GetType().FullName);
            }

            var filename = message.Header.GetHeaderItem(FileHeaders.FileName.ToString());
            if (!(filename is NullEnvelopeHeaderItem))
                file = filename.GetValue<string>();

            using (var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.Write(payload, 0, payload.Length);
            }
        }

        /// <summary>
        /// This will place the contents to the target location as defined for the uri location.
        /// </summary>
        /// <param name="destination">Uri defining the directory to delivery the information</param>
        /// <param name="message">Message to be delivered.</param>
        /// <returns></returns>
        private string CreateFileForDelivery(string destination, IEnvelope message)
        {
            FileInfo fileInfo = null;
            var file = string.Empty;
            var targetLocation = FileAdapterUtils.RetreiveLocationFromProtocolUri(this.GetScheme(), destination);

            if (this.FileNameStrategy != null)
                file = Path.Combine(targetLocation,
                                    string.Concat(this.FileNameStrategy.FileName, this.FileNameStrategy.FileExtension));
            else
            {
                file = Path.Combine(targetLocation,
                    FileAdapterUtils.CreateDefaultFileName(message.Header.MessageId, message.Header.CorrelationId));   
            }

            fileInfo = new FileInfo(file);

            return fileInfo.FullName;
        }
    }
}