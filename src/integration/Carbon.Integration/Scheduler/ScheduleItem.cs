using System;
using System.Threading;
using Carbon.Core;

namespace Carbon.Integration.Scheduler
{
    public class ScheduleItem : IScheduledItem
    {
        private bool m_disposed;

        public event EventHandler<ScheduledItemCompletedEventArgs> ScheduledItemCompleted;

        public event EventHandler<ScheduledItemErrorEventArgs> ScheduledItemError;

        public Timer Timer
        {
            get;
            set;
        }

        public IScheduledTask Task
        {
            get;
            set;
        }

        public bool IsRunning { get; private set; }

        public void Start()
        {
            if(m_disposed)
                return;

            if(IsRunning)
                return;

            Task.ScheduledTaskExecuted += ScheduledTaskExecuted;
            Task.ScheduledTaskError += ScheduledTaskError;

            Timer = new Timer(ExecuteTask, null, 100,  Task.Frequency * 1000);

            IsRunning = true;
        }

        public void Stop()
        {
            if (this.Timer != null)
                Timer.Dispose();

            Task.ScheduledTaskExecuted -= ScheduledTaskExecuted;
            Task.ScheduledTaskError -= ScheduledTaskError;
            IsRunning = false;
        }

        private void ExecuteTask(object state)
        {
            try
            {
                if (m_disposed) return;
                this.Task.Execute();
            }
            catch (Exception exception)
            {
                if (!OnScheduledItemError(exception))
                    throw;
            }

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);      
        }

        private void Dispose(bool disposing)
        {
            if(!m_disposed)
            {
                if(disposing)
                {
                    this.Stop();
                }

                m_disposed = true;
            }
        }

        private void ScheduledTaskError(object sender, ScheduledTaskErrorEventArgs e)
        {
            var msg = string.Format("Instance: {0}, Method: {1} Error: {2}", e.InstanceName, e.MethodName,
                                    e.Exception.Message);
            OnScheduledItemError(new Exception(msg, e.Exception));
        }

        private void ScheduledTaskExecuted(object sender, ScheduledTaskExecutedEventArgs e)
        {
            OnScheduledItemCompleted(e.Message);
        }

        private bool OnScheduledItemError(Exception exception)
        {
            EventHandler<ScheduledItemErrorEventArgs> evt = this.ScheduledItemError;

            var isEventHandlerAttached = (evt != null);

            if (isEventHandlerAttached)
                evt(this, new ScheduledItemErrorEventArgs(exception));

            return isEventHandlerAttached;
        }

        private void OnScheduledItemCompleted(IEnvelope message)
        {
            EventHandler<ScheduledItemCompletedEventArgs> evt = this.ScheduledItemCompleted;
            if (evt != null)
                evt(this, new ScheduledItemCompletedEventArgs(message));
        }
    }
}