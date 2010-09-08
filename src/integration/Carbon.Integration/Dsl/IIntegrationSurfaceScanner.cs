using System;
using System.Collections.ObjectModel;

namespace Carbon.Integration.Dsl
{
    /// <summary>
    /// Contract for inspecting all libraries for integration surface components and registering them for use.
    /// </summary>
    public interface IIntegrationSurfaceScanner
    {
        /// <summary>
        /// This will scan all libraries in the current executable directory for all surfaces that have components
        /// for participating in messaging scenarios.
        /// </summary>
        void Scan();
    }
}