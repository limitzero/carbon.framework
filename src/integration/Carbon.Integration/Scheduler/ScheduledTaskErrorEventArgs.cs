using System;

namespace Carbon.Integration.Scheduler
{
    ///<summary>
    /// <see cref="EventArgs"/> for when an error is encountered for a scheduled task.
    ///</summary>
    public class ScheduledTaskErrorEventArgs : EventArgs
    {
        public string InstanceName { get; set; }
        public string MethodName { get; set; }
        public Exception Exception { get; set; }

        public ScheduledTaskErrorEventArgs(string instanceName, string methodName, Exception exception)
        {
            InstanceName = instanceName;
            MethodName = methodName;
            Exception = exception;
        }
    }
}