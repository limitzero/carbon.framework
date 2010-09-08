namespace Carbon.Core.Channel.Impl.Null
{
    public class NullChannel : AbstractChannel
    {
        public override IEnvelope DoReceive()
        {
            // noop
            return new NullEnvelope();
        }

        public override void DoSend(IEnvelope envelope)
        {
            //noop
        }
    }
}