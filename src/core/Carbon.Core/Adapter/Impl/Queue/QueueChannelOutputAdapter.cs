using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using Carbon.Core.Builder;
using Carbon.Channel.Registry;
using Carbon.Core.Channel.Impl.Null;

namespace Carbon.Core.Adapter.Impl.Queue
{
    /// <summary>
    /// Adapter for taking information from a channel and loading it to another channel location.
    /// Addressing Scheme : vm://{channel name}
    /// </summary>
    public class QueueChannelOutputAdapter : 
        AbstractOutputChannelAdapter
    {
        public QueueChannelOutputAdapter(IObjectBuilder builder)
            : base(builder)
        {
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
            catch 
            {
                throw;
            }
        }

        private void SubmitMessage(string destination, IEnvelope message)
        {
            var channel = QueueChannelAdapterUtils.
                RetreiveLocationFromProtocolUri(this.GetScheme(), destination);

            if(!(base.ObjectBuilder.Resolve<IChannelRegistry>().FindChannel(channel) is NullChannel))
                base.ObjectBuilder.Resolve<IChannelRegistry>().FindChannel(channel).Send(message);
        }

    }
}