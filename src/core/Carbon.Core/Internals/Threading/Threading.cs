using System;
using System.Threading;

namespace Carbon.Core.Internals.Threading
{
    public delegate void Action();

    /// <summary>
    /// Represents a worker thread that will repeatedly execute a callback.
    /// </summary>
    public class WorkerThread
    {
        private readonly Action m_methodToRunInLoop;
        private readonly object m_toLock = new object();
        private readonly Thread m_thread;
        private bool m_isStopRequested;

        /// <summary>
        /// Initializes a new WorkerThread for the specified method to run.
        /// </summary>
        /// <param name="methodToRunInLoop">The delegate method to execute in a loop.</param>
        public WorkerThread(Action methodToRunInLoop)
        {
            m_methodToRunInLoop = methodToRunInLoop;
            m_thread = new Thread(Loop);
            m_thread.SetApartmentState(ApartmentState.MTA);
            m_thread.Name = string.Format("Worker.{0}", m_thread.ManagedThreadId);
            m_thread.IsBackground = true;
        }

        public event EventHandler Stopped;

        public bool StopRequested
        {
            get
            {
                bool result;
                lock (m_toLock)
                {
                    result = m_isStopRequested;
                }
                return result;
            }
        }

        public void Start()
        {
            if (!m_thread.IsAlive)
                m_thread.Start();
        }

        public void Stop()
        {
            lock (m_toLock)
                m_isStopRequested = true;
        }

        protected void Loop()
        {
            while (!StopRequested)
            {
                try
                {
                    if (m_methodToRunInLoop != null)
                        m_methodToRunInLoop();
                }
                catch
                {
                    
                }
            }

            if (StopRequested)
            {
                m_thread.Join();
                OnStopRequested();
            }
        }

        #region -- private methods --

        private void OnStopRequested()
        {
            if (Stopped != null)
                Stopped(this, new EventArgs());
        }

        #endregion

    }

    public class WorkerThreadPool
    {
        private readonly int m_thread_count;
        private readonly Action m_method_to_run_in_loop;
        private object m_lock = new object();
        private WorkerThread[] m_workerThreads = { };
        private bool m_running;

        public WorkerThreadPool(int threadCount, Action methodToRunInLoop)
        {
            m_thread_count = threadCount;
            m_method_to_run_in_loop = methodToRunInLoop;
            m_workerThreads = new WorkerThread[threadCount];
        }

        public void StartService()
        {
            if(m_running)
                return;

            for (int index = 0; index < m_thread_count; index++)
            {
                WorkerThread t = new WorkerThread(m_method_to_run_in_loop);
                t.Stopped += ThreadStopped;
                t.Start();
                m_workerThreads[index] = t;
            }
            m_running = true;
        }

        public void StopService()
        {
            foreach (WorkerThread thread in m_workerThreads)
            {
                thread.Stop();
            }
            m_running = false;
        }

        public bool IsRunning
        {
            get
            {
                bool result;
                lock (m_lock)
                {
                    result = m_running;
                }
                return m_running;
            }
        }

        private void ThreadStopped(object sender, EventArgs eventArgs)
        {

        }
    }
}