using System;

namespace Carbon.Core.Stereotypes.For.Components.MessageEndpoint
{
    /// <summary>
    /// Attribute for a indicating that it will contain methods for handling messages over 
    /// a channel with behavior (if necessary) added to the channel for processing.
    /// </summary>
    /// <example>
    /// [MessagingEndpoint("billing", "shipping")]
    /// public class InventoryService
    /// {
    /// }
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageEndpointAttribute : Attribute
    {
        /// <summary>
        /// (Read-Write). The channel that is dedicated to this component for receiving messages.
        /// </summary>
        public string InputChannel { get; set; }

        /// <summary>
        /// (Read-Write). The channel that is interested in the messages produced by this service.
        /// </summary>
        public string OutputChannel { get; set; }

        /// <summary>
        /// Attribute for a indicating that it will contain methods for handling messages with behavior 
        /// applied to the channel (if necessary).
        /// </summary>
        /// <param name="inputChannel">The name of the channel that the end point will listen for messages.</param>
        public MessageEndpointAttribute(string inputChannel)
            : this(inputChannel, string.Empty)
        {
        }

        /// <summary>
        /// Attribute for a indicating that it will contain methods for handling messages with behavior 
        /// applied to the channel (if necessary).
        /// </summary>
        /// <param name="inputChannel">The channel that is dedicated to this component for receiving messages.</param>
        /// <param name="outputChannel">The channel that is interested in the messages produced by this service.</param>
        public MessageEndpointAttribute(string inputChannel, string outputChannel)
        {
            InputChannel = inputChannel;
            OutputChannel = outputChannel;
        }
    }
}