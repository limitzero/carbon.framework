using System;
using Carbon.Core;

namespace Carbon.Integration.Scheduler
{
    public class SchedulerItemCompletedEventArgs  : EventArgs
    {
        public IEnvelope Message { get; set; }

        public SchedulerItemCompletedEventArgs(IEnvelope message)
        {
            Message = message;
        }
    }
}