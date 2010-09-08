using System;
using System.Threading;
using Carbon.Core.RuntimeServices;

namespace Carbon.Integration.Scheduler
{
    public interface IScheduledItem : IStartable
    {
        event EventHandler<ScheduledItemCompletedEventArgs> ScheduledItemCompleted;
        event EventHandler<ScheduledItemErrorEventArgs> ScheduledItemError;

        Timer Timer { get; set; }
        IScheduledTask Task { get; set; }
    }
}