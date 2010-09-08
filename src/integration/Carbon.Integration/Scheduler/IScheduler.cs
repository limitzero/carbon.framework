using System;
using System.Collections.Generic;
using System.Text;
using Carbon.Core.RuntimeServices;

namespace Carbon.Integration.Scheduler
{
    /// <summary>
    /// Contract for the task scheduler that will inspect items with the <see cref="PolledAttribute"/>
    /// annotation on the method for scheduled execution.
    /// </summary>
    public interface IScheduler : IStartable
    {
        /// <summary>
        /// Event that is triggered when the scheduled item has been executed.
        /// </summary>
        event EventHandler<SchedulerItemCompletedEventArgs> SchedulerItemCompleted;

        /// <summary>
        /// Event that is triggered when the scheduled item has generated an error.
        /// </summary>
        event EventHandler<ScheduledItemErrorEventArgs> SchedulerItemError;

        /// <summary>
        /// (Read-Write). The collection of scheduled items that are set for execution.
        /// </summary>
        IList<IScheduledItem> RegisteredItems { get; set; }

        /// <summary>
        /// This will register an item in the scheduler for execution.
        /// </summary>
        /// <param name="item"></param>
        void RegisterItem(IScheduledItem item);

        /// <summary>
        /// This will inspect the channel registry and configure all 
        /// methods that should be "polled" on a periodic basis.
        /// </summary>
        /// <param name="commponentTypes"></param>
        void ScanAndRegister(IList<Type> commponentTypes);
    }
}