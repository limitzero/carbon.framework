using System;

namespace Carbon.Core.RuntimeServices
{
    /// <summary>
    /// Contract for any service that will run in the background.
    /// </summary>
    public interface IBackgroundService : IStartable
    {
        /// <summary>
        /// Event that is triggered when the background service is started.
        /// </summary>
        event EventHandler<BackGroundServiceEventArgs> BackgroundServiceStarted;

        /// <summary>
        /// Event that is triggered when the background service has stopped.
        /// </summary>
        event EventHandler<BackGroundServiceEventArgs> BackgroundServiceStopped;

        /// <summary>
        /// Event that is triggered when the background service encounters an error.
        /// </summary>
        event EventHandler<BackGroundServiceErrorEventArgs> BackgroundServiceError;

        /// <summary>
        /// (Read-Write). The number of active threads polling the location looking for new messages.
        /// </summary>
        int Concurrency { get; set; }

        /// <summary>
        /// (Read-Write). The interval, in seconds, that each thread should wait before polling the location for messages.
        /// </summary>
        int Frequency { get; set; }

        /// <summary>
        /// (Read-Write). The interval or "schedule", in seconds, that the location will be polled looking for new messages.
        /// </summary>
        int Interval { get; set; }
    }
}