using System;
using Carbon.Core.Builder;

namespace Carbon.Core.Adapter.Registry
{
    /// <summary>
    /// Contract representing the configuration of an adapter for registration.
    /// </summary>
    public interface IAdapterConfiguration
    {
        /// <summary>
        /// (Read-Only). The scheme associated with the adapter for creation and message transmission based on Uri conventions.
        /// </summary>
        string Scheme { get; }

        /// <summary>
        /// (Read-Only). The sample uri addressing scheme for the adapter.
        /// </summary>
        string Uri { get;  }

        /// <summary>
        /// (Read-Only). The adapter that will take a message from location and load it into a channel for processing.
        /// </summary>
        Type InputChannelAdapter { get; }

        /// <summary>
        /// (Read-Only). The adapter that will take a message from a channel and load it into an offline location for processing.
        /// </summary>
        Type OutputChannelAdapter { get; }

        /// <summary>
        /// This will configure the input and output adapters based on conventions for use in messaging.
        /// </summary>
        /// <returns></returns>
        IAdapterConfiguration Configure(IObjectBuilder builder);
    }
}