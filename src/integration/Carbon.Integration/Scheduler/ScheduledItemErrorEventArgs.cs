using System;

namespace Carbon.Integration.Scheduler
{
    ///<summary>
    ///</summary>
    public class ScheduledItemErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        public ScheduledItemErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}