using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using Carbon.Core;
using Carbon.Core.Adapter;
using Carbon.Core.Builder;

namespace Carbon.Stdio.Adapter
{
    /// <summary>
    ///  Addressing scheme: stdio://local/?prompt={any message}
    /// </summary>
    public class StdioOutputChannelAdapter : AbstractOutputChannelAdapter
    {
        public StdioOutputChannelAdapter(IObjectBuilder builder) : base(builder)
        {
        }

        public override void DoSend(IEnvelope envelope)
        {
            this.Send(envelope);
        }

        private void Send(IEnvelope message)
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
            // write the contents to the stdio console:
            Console.WriteLine(message.Body.GetPayload<string>());
        }

    }
}