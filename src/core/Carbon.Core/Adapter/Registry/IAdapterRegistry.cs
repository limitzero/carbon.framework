using System;
using System.Collections.Generic;
using System.Reflection;
using Carbon.Core.RuntimeServices;

namespace Carbon.Core.Adapter.Registry
{
    /// <summary>
    /// Contract for storing all adapters that will be used for communication.
    /// </summary>
    public interface IAdapterRegistry : IStartable
    {
        /// <summary>
        /// (Read-Write). The collection of input adapters that will take a message from a physical location 
        /// and load it to a channel for processing.
        /// </summary>
        IList<AbstractInputChannelAdapter> InputAdapters { get; }

        /// <summary>
        /// (Read-Write). The collection of output adapters that will take a message from a channel and deliver
        /// it to a physical location.
        /// </summary>
        IList<AbstractOutputChannelAdapter> OutputAdapters { get; }

        /// <summary>
        /// This will scan an assebly by name for all adapters that can be registered for use.
        /// </summary>
        /// <param name="asssemblyName">Name of the assembly to scan.</param>
        void Scan(string asssemblyName);

        /// <summary>
        /// This will scan an assebly for all adapters that can be registered for use.
        /// </summary>
        /// <param name="assembly">Assembly to scan</param>
        void Scan(Assembly assembly);

        /// <summary>
        /// This will register the adapter configuration in the registry for later 
        /// access on building.
        /// </summary>
        /// <param name="configuration"></param>
        void RegisterAdapterConfiguration(IAdapterConfiguration configuration);

        /// <summary>
        /// This will inspect the uri and based on the schemes for adapters 
        /// that are registered, create the input adapter instance.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>
        /// The type of the concrete instance of the input adapter defined by the uri scheme.
        /// </returns>
        AbstractInputChannelAdapter FindInputAdapterFromUriScheme(string uri);

        /// <summary>
        /// This will inspect the uri and based on the schemes for adapters 
        /// that are registered, create the output adapter instance.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>
        /// The type of the concrete instance of the output adapter defined by the uri scheme.
        /// </returns>
        AbstractOutputChannelAdapter FindOutputAdapterFromUriScheme(string uri);

        /// <summary>
        /// This will register a pre-build input channel adapter configuration.
        /// </summary>
        /// <param name="adapter"></param>
        void RegisterInputChannelAdapter(AbstractInputChannelAdapter adapter);

        /// <summary>
        /// This will register a pre-build output channel adapter configuration.
        /// </summary>
        /// <param name="adapter"></param>
        void RegisterOutputChannelAdapter(AbstractOutputChannelAdapter adapter);
    }
}