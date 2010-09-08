using System;
using System.Threading;
using System.Timers;
using System.Transactions;
using Carbon.Core.Internals.Threading;
using Timer = System.Timers.Timer;

namespace Carbon.Core.RuntimeServices
{
    /// <summary>
    /// Abstract class for all services that must run in the background.
    /// </summary>
    public abstract class AbstractBackgroundService : IBackgroundService
    {
        private WorkerThreadPool m_pool = null;
        private Timer m_schedule = null;
        private bool m_disposed;

        public event EventHandler<BackGroundServiceEventArgs> BackgroundServiceStarted;
        public event EventHandler<BackGroundServiceEventArgs> BackgroundServiceStopped;
        public event EventHandler<BackGroundServiceErrorEventArgs> BackgroundServiceError;

        /// <summary>
        /// (Read-Write). The name of the service instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// (Read-Write). The number of concurrent threads used for processing.
        /// </summary>
        public int Concurrency { get; set; }

        /// <summary>
        /// (Read-Write). The interval, in seconds, that each thread should wait before polling the location for messages.
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// (Read-Write). The interval or "schedule", in seconds, that the location will be polled looking for new messages.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// (Read-Only). Flag to indicate whether the component is started or not.
        /// </summary>
        public virtual bool IsRunning
        {
            get;
            private set;
        }

        public virtual void Start()
        {
            if (IsRunning)
                return;

            if (m_disposed)
                return;

            var threads = Concurrency > 0 ? Concurrency : 1;
            this.Frequency = this.Frequency == 0 ? 100 : this.Frequency * 1000;

            if (Interval > 0)
            {
                m_schedule = new Timer(this.Interval * 1000);
                m_schedule.Elapsed += ScheduleElasped;
                m_schedule.Start();
            }
            else
            {
                m_pool = new WorkerThreadPool(threads, () => this.Execute());
                m_pool.StartService();
            }

            IsRunning = true;
            OnServiceStarted();
        }

        public virtual void Stop()
        {
            if (m_pool != null)
                m_pool.StopService();

            if (m_schedule != null)
            {
                m_schedule.Stop();
                m_schedule.Elapsed -= ScheduleElasped;
                m_schedule.Dispose();
            }

            IsRunning = false;
            OnServiceStopped();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    this.Stop();
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// This will be invoked in a periodic fashion for the 
        /// custom service code to perform some actions specific 
        /// to their design.
        /// </summary>
        public abstract void PerformAction();

        public bool OnServiceError(string message, Exception exception)
        {
            EventHandler<BackGroundServiceErrorEventArgs> evt = this.BackgroundServiceError;
            var isHandlerAttached = (evt != null);

            if (isHandlerAttached)
                evt(this, new BackGroundServiceErrorEventArgs(message, exception));

            return isHandlerAttached;
        }

        private void Execute()
        {
            try
            {
                if (m_disposed) return;

                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(this.Frequency));
                this.PerformAction();
            }
            catch (ThreadAbortException tex)
            {
                // ignore
            }
            catch (TransactionAbortedException tex)
            {
                if (!OnServiceError(tex.Message, tex))
                    throw;
            }
            catch (Exception exception)
            {
                if (!OnServiceError(exception.Message, exception))
                    throw;
            }
        }

        private void ScheduleElasped(object sender, ElapsedEventArgs e)
        {
            this.Execute();
        }

        private void OnServiceStarted()
        {
            EventHandler<BackGroundServiceEventArgs> evt = this.BackgroundServiceStarted;
            if (evt != null)
                evt(this, new BackGroundServiceEventArgs(string.Format("{0} service started.", this.Name)));
        }

        private void OnServiceStopped()
        {
            EventHandler<BackGroundServiceEventArgs> evt = this.BackgroundServiceStopped;
            if (evt != null)
                evt(this, new BackGroundServiceEventArgs(string.Format("{0} service stopped.", this.Name)));
        }
    }
}