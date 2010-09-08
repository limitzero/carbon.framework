using Carbon.Core;

namespace Carbon.Integration.Pipeline.Component
{
    /// <summary>
    /// Contract for all components that will be used to pre or post process a message that 
    /// is either received or sent via an adapter.
    /// </summary>
    public interface IPipelineComponent
    {
        /// <summary>
        /// (Read-Write). Name of the component used in the message processing pipeline.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// This will execute the component used in the messaging pipeline.
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        IEnvelope Execute(IEnvelope envelope);
    }
}