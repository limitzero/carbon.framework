namespace Carbon.Core.Pipeline
{
    public enum PipelineDirection
    {
        Send,
        Receive
    }

    /// <summary>
    /// The pipeline is responsible for mantaining message consistency 
    /// across the adapter to end point invocation specifically for transaction support
    /// and common message handling routines before the message is sent to the 
    /// destination and after the messsage is received from a location.
    /// </summary>
    public interface  IPipeline
    {
        IEnvelope Invoke(PipelineDirection direction, IEnvelope envelope);
    }
}