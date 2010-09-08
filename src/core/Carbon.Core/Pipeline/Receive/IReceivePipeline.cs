using Carbon.Core.Pipeline.Component;

namespace Carbon.Core.Pipeline.Receive
{
    /// <summary>
    /// Contract pipeline used for all messages that are received from the physical 
    /// location via the input adapter.
    /// </summary>
    public interface IReceivePipeline : IPipeline
    {
        /// <summary>
        /// (Read-Write). Name of the component used in the message processing pipeline.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// This will register all components that will be used for processing the 
        /// message from the physical location before it is put on the channel 
        /// for processing. The components will be executed in the order 
        /// that they are registered.
        /// </summary>
        /// <param name="components">Components used to process the message after receipt from the physical location.</param>
        void RegisterComponents(params AbstractPipelineComponent[] components);

        /// <summary>
        /// This will register all components that will be used for processing the 
        /// message from the physical location before it is put on the channel 
        /// for processing. The components will be executed in the order 
        /// that they are registered. The objects must implement the base class
        /// of <seealso cref="AbstractPipelineComponent"/> in order to be successfully 
        /// registered.
        /// </summary>
        /// <param name="components">Components used to process the message after receipt from the physical location.</param>
        void RegisterComponents(params object[] components);

    }
}