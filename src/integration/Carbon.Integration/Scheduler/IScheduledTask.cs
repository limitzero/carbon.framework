using System;
using System.Collections.Generic;
using System.Text;

namespace Carbon.Integration.Scheduler
{
    /// <summary>
    /// Contract for a scheduled task.
    /// </summary>
    public interface IScheduledTask
    {
        /// <summary>
        /// Event that is triggered when the scheduled task has completed execution.
        /// </summary>
        event EventHandler<ScheduledTaskExecutedEventArgs> ScheduledTaskExecuted;

        /// <summary>
        /// Event that is triggered when the scheduled task has completed execution.
        /// </summary>
        event EventHandler<ScheduledTaskErrorEventArgs> ScheduledTaskError;

        /// <summary>
        /// (Read-Write). The interval, in seconds, that the method on the component should be polled.
        /// </summary>
        int Frequency { get; set; }

        /// <summary>
        /// This will start the scheduled task.
        /// </summary>
        void Execute();
    }
}