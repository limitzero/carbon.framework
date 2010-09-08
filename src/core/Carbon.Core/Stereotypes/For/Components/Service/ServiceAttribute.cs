using System;

namespace Carbon.Core.Stereotypes.For.Components.Service
{
    /// <summary>
    /// Attribute to indicate that a service layer component is being activated via messaging.
    /// The service stereotype (attribute) denotes a point in the application where a message
    /// will be received over a channel but no behavior will be applied to the channel while 
    /// processing the message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// (Read-Write). The channel that is dedicated to this servoce for receiving messages.
        /// </summary>
        public string InputChannel { get; private set; }

        /// <summary>
        /// (Read-Write). The channel that is interested in the messages produced by this service.
        /// </summary>
        public string OutputChannel { get; private set; }

        /// <summary>
        /// Attribute for a indicating that it will contain methods for handling messages.
        /// </summary>
        /// <param name="inputChannel">The name of the channel that the service will listen for messages.</param>
        public ServiceAttribute(string inputChannel)
            : this(inputChannel, string.Empty)
        {
        }

        /// <summary>
        /// Attribute for a indicating that it will contain methods for handling messages.
        /// </summary>
        /// <param name="inputChannel">The name of the channel that the service will listen for messages.</param>
        /// <param name="outputChannel">The channel that is interested in the messages produced by this service.</param>
        public ServiceAttribute(string inputChannel, string outputChannel)
        {
            InputChannel = inputChannel;
            OutputChannel = outputChannel;
        }
    }
}