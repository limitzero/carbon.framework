using Castle.MicroKernel;

namespace Carbon.Integration.Tests.Api.Surface.Ports
{
    /// <summary>
    /// Contract for a input or output adapter for accepting 
    /// or sending a message to a physical location.
    /// </summary>
    public interface IPort
    {
        /// <summary>
        ///  (Read-Write). Instance of container.
        /// </summary>
        IKernel Kernel { set; get; }
        
        string Channel { get; }
        string Uri { get; }
        int Concurrency { get;  }
        int Frequency { get;  }
        int Schedule { get;  }

        /// <summary>
        /// Creates an adapter with a channel name and uri.
        /// </summary>
        /// <param name="channel">Name of the channel that will hold the message</param>
        /// <param name="uri">Location of the physical location that will be used to retrieve or store a message.</param>
        IPort CreatePort(string channel, string uri);

        /// <summary>
        /// Creates an adapter with a channel name, uri,  concurrency (number of active worker processes), and 
        /// frequency (time between worker process requests).
        /// </summary>
        /// <param name="channel">Name of the channel that will hold the message</param>
        /// <param name="uri">Location of the physical location that will be used to retrieve or store a message.</param>
        /// <param name="concurrency">Number of active worker processes used to poll the location for messages</param>
        /// <param name="frequency">Number of seconds in between each worker process request.</param>
        IPort CreatePort(string channel, string uri, int concurrency, int frequency);

        /// <summary>
        /// Creates an adapter with a channel name, uri,  and schedule.
        /// </summary>
        /// <param name="channel">Name of the channel that will hold the message</param>
        /// <param name="uri">Location of the physical location that will be used to retrieve or store a message.</param>
        /// <param name="scheduled">Number of seconds that the location will be polled.</param>
        IPort CreatePort(string channel, string uri, int scheduled);

        /// <summary>
        /// This will build the adapter from the configuration settings set by the ConfigureAdapter methods.
        /// </summary>
        void Build();
    }
}