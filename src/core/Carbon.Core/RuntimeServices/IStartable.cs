using System;
using System.Collections.Generic;
using System.Text;

namespace Carbon.Core.RuntimeServices
{
    /// <summary>
    /// The <seealso cref="IStartable"/> interface is applied to all components that can be started/stopped 
    /// for runtime activity.
    /// </summary>
    public interface IStartable : IDisposable
    {
        /// <summary>
        /// (Read-Only). Flag to indicate whether the component is started or not.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the component for processing.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the component from processing.
        /// </summary>
        void Stop();
    }
}