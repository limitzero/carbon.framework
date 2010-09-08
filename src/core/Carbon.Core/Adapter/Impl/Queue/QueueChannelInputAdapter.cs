using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Carbon.Channel.Registry;
using Carbon.Core.Builder;
using Carbon.Core.Channel.Impl.Null;

namespace Carbon.Core.Adapter.Impl.Queue
{
    /// <summary>
    /// Adapter for taking messages from an in-memory location and loading them onto a channel.
    /// Addressing Scheme : vm://{channel name}
    /// </summary>
    public class QueueChannelInputAdapter :  AbstractInputChannelAdapter
    {
        public QueueChannelInputAdapter(IObjectBuilder builder)
            :base(builder)
        {
            
        }

        public override Tuple<IEnvelopeHeader, byte[]> DoReceive()
        {
            IEnvelope envelope = new NullEnvelope(); 

            var channel = QueueChannelAdapterUtils.
              RetreiveLocationFromProtocolUri(this.GetScheme(), this.Uri);

            if (!(base.ObjectBuilder.Resolve<IChannelRegistry>().FindChannel(channel) is NullChannel))
                envelope = base.ObjectBuilder.Resolve<IChannelRegistry>().FindChannel(channel).Receive();

            base.SetMessageForReceive(envelope);

            // this will be ignored:
            var contents = this.ExtractMessageContents();
            var header = this.CreateMessageHeader();
            var tuple = new Tuple<IEnvelopeHeader, byte[]>(header, contents);

            return tuple;

        }

        public override byte[] ExtractMessageContents()
        {
            return null;
        }

        public override IEnvelopeHeader CreateMessageHeader()
        {
            return null;
        }

    }
}