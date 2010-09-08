namespace Carbon.Core
{
    public class NullEnvelopeHeaderItem : EnvelopeHeaderItem
    {
        public NullEnvelopeHeaderItem()
            :base(string.Empty, string.Empty)
        {
        }

        protected NullEnvelopeHeaderItem(string name, object value) : 
            base(name, value)
        {

        }
    }
}