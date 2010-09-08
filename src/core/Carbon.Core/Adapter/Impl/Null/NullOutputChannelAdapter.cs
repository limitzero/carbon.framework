using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Impl.Null
{
    /// <summary>
    /// noop
    /// </summary>
    public class NullOutputChannelAdapter : AbstractOutputChannelAdapter
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public NullOutputChannelAdapter()
            :this(null)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="builder"></param>
        public NullOutputChannelAdapter(IObjectBuilder builder) : base(builder)
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
        public override void DoSend(IEnvelope envelope)
        {
            return;
        }
    }
}