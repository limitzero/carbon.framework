using System;

namespace Carbon.ESB
{
    /// <summary>
    /// The endpoint contract represents a mapping of a physical location to a component for processing messages.
    /// </summary>
    public interface IEndpoint
    {
        /// <summary>
        /// (Read-Write). The name of the endpoint (corresponds to the channel name)
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// (Read-Write). The location of the messaging endpoint specified using a Uri convention.
        /// </summary>
        Uri Uri { get; set; }

        /// <summary>
        /// (Read-Write). The fully qualified assembly name of the component that will be processing messages
        /// at the given endpoint location.
        /// </summary>
        string Component { get; set; }

        /// <summary>
        /// (Read-Write). The set of messages that the endpoint can process.
        /// </summary>
        string[] Messages { get; set; }

        /// <summary>
        /// This will build the endpoint messages collection for the component type passed.
        /// </summary>
        /// <param name="endpoint">Type of the component endpoint used for processing messages.</param>
        void Configure(Type endpoint);
    }

    public class Endpoint : IEndpoint
    {
        public string Name
        {
            get;
            set;
        }

        public Uri Uri
        {
            get; set;
        }

        public string Component
        {
            get; set;
        }

        public string[] Messages
        {
            get;
            set;
        }

        public void Configure(Type endpoint)
        {
            throw new NotImplementedException();
        }
    }
}