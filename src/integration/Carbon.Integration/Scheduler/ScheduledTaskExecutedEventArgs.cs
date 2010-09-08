using System;
using Carbon.Core;

namespace Carbon.Integration.Scheduler
{
    public class ScheduledTaskExecutedEventArgs : EventArgs
    {
        public IEnvelope Message { get; set; }

        public ScheduledTaskExecutedEventArgs(IEnvelope message)
        {
            Message = message;
        }
    }
}