using Carbon.Core.Pipeline.Component;

namespace Carbon.Core.Pipeline.Send
{
    /// <summary>
    /// Contract pipeline used for all messages that are received from a channel
    /// and sent to a physical location via an output channel adapter.
    /// </summary>
    public interface ISendPipeline : IPipeline
    {
        /// <summary>
        /// This will register all components that will be used for processing the 
        /// message from the channel before it it sent to the physical location
        /// for processing. The components will be executed in the order 
        /// that they are registered.
        /// </summary>
        /// <param name="components">Components used to process the message before sending to the physical location.</param>
        void RegisterComponents(params AbstractPipelineComponent[] components);

        /// <summary>
        /// This will register all components that will be used for processing the 
        /// message from the channel before it it sent to the physical location
        /// for processing. The components will be executed in the order 
        /// that they are registered. The objects must implement the base class
        /// of <seealso cref="AbstractPipelineComponent"/> in order to be successfully 
        /// registered.
        /// </summary>
        /// <param name="components">Components used to process the message before sending to the physical location.</param>
        void RegisterComponents(params object[] components);
    }
}