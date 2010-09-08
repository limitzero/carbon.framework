using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Impl.Null
{
    /// <summary>
    /// noop
    /// </summary>
    public class NullInputChannelAdapter : AbstractInputChannelAdapter
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public NullInputChannelAdapter()
            :this(null)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public NullInputChannelAdapter(IObjectBuilder builder) : base(builder)
        {
        }

        /// <summary>
        /// noop
        /// </summary>
        public override void Start()
        {
            return;
        }

        /// <summary>
        /// noop
        /// </summary>
        public override void Stop()
        {
            return;
        }

        /// <summary>
        /// noop
        /// </summary>
        public override Tuple<IEnvelopeHeader, byte[]> DoReceive()
        {
            var tuple = new Tuple<IEnvelopeHeader, byte[]>(null, null);
            return tuple;
        }

        /// <summary>
        /// noop
        /// </summary>
        public override IEnvelopeHeader CreateMessageHeader()
        {
            return null;
        }

        /// <summary>
        /// noop
        /// </summary>
        public override byte[] ExtractMessageContents()
        {
            return null;
        }
    }
}