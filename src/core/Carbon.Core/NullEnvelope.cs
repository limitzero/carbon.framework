namespace Carbon.Core
{
    /// <summary>
    /// Implementatoin of "null object" pattern for the <seealso cref="IEnvelope"/>
    /// </summary>
    public class NullEnvelope : Envelope
    {
        public NullEnvelope()
            :base(null)
        {
            
        }
    }
}