using System;
using Carbon.Core;

namespace Carbon.Integration.Scheduler
{
    /// <summary>
    /// <see cref="EventArgs"/> that are triggered by the <see cref="Scheduler"/>
    /// </summary>
    public class ScheduledItemCompletedEventArgs : EventArgs
    {
        public IEnvelope Message { get; set; }

        public ScheduledItemCompletedEventArgs(IEnvelope message)
        {
            Message = message;
        }
    }
}