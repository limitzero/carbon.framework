using System;
using System.Collections.Generic;
using System.Reflection;
using Carbon.Core;
using Carbon.Integration.Stereotypes.Polled;
using Castle.MicroKernel;
using Carbon.Core.Adapter.Template;
using Carbon.Core.Builder;

namespace Carbon.Integration.Scheduler
{
    /// <summary>
    /// The scheduler is the point in the system where all pollable tasks are registered for execution 
    /// according to their defined intervals.
    /// </summary>
    public class Scheduler : IScheduler
    {
        private readonly IObjectBuilder _object_builder;
        private List<IScheduledItem> m_scheduled_items;
        private bool m_disposed;

        public event EventHandler<SchedulerItemCompletedEventArgs> SchedulerItemCompleted;
        public event EventHandler<ScheduledItemErrorEventArgs> SchedulerItemError;

        public IList<IScheduledItem> RegisteredItems
        {
            get { return m_scheduled_items; }
            set { m_scheduled_items = new List<IScheduledItem>(value); }
        }

        public bool IsRunning { get; private set; }

        public Scheduler(IObjectBuilder builder)
        {
            _object_builder = builder;
            m_scheduled_items = new List<IScheduledItem>();
        }

        public void Start()
        {
            if(m_disposed) return;

            foreach (var item in RegisteredItems)
            {
                item.ScheduledItemCompleted += ScheduledItem_Completed;
                item.ScheduledItemError += ScheduledItem_Error;
                item.Start();
            }
            IsRunning = true;
            OnSchedulerStarted();
        }

        public void Stop()
        {
            foreach (var item in m_scheduled_items)
            {
                item.ScheduledItemCompleted -= ScheduledItem_Completed;
                item.ScheduledItemError -= ScheduledItem_Error;
                item.Stop();
            }
            IsRunning = false;
            OnSchedulerStopped();
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

        public void RegisterItem(IScheduledItem item)
        {
            RegisteredItems.Add(item);
        }

        public void ScanAndRegister(IList<Type> componentTypes)
        {
            if(m_disposed) return;

            foreach (var componentType in componentTypes)
                if (componentType.IsClass && !componentType.IsAbstract)
                {
                    foreach (var method in componentType.GetMethods())
                        foreach (var attribute in method.GetCustomAttributes(true))
                        {
                            if (attribute.GetType() == typeof (PolledAttribute))
                                BuildScheduledItemFor(componentType, method);
                        }
                }
        }

        private void BuildScheduledItemFor(Type component, MethodInfo method)
        {
            var pollableAttribute = method.GetCustomAttributes(typeof (PolledAttribute), true)[0];

            if(pollableAttribute == null)
                return;

            var duration = ((PolledAttribute) pollableAttribute).Interval;

            var task = new MethodInvokerScheduledTask();

            _object_builder.Register(component.Name, component);
            
            task.Instance = _object_builder.Resolve(component);
            task.Method = method;
            task.Frequency = duration.Seconds;

            var scheduledItem = new ScheduleItem() { Task = task };
            this.RegisterItem(scheduledItem);
        }

        private void OnSchedulerStarted()
        {
            try
            {
                var messageTemplate = _object_builder.Resolve<IAdapterMessagingTemplate>();

                if (messageTemplate == null) return;

                messageTemplate.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI),
                                       new Envelope("Scheduler started."));
            }
            catch (Exception exception)
            {
                if (!OnSchedulerError(null, exception))
                    throw;
            }
        }

        private void OnSchedulerStopped()
        {
            try
            {
                var messageTemplate = _object_builder.Resolve<IAdapterMessagingTemplate>();

                if(messageTemplate == null) return;

                messageTemplate.DoSend(new Uri(Constants.LogUris.INFO_LOG_URI),
                                       new Envelope("Scheduler stopped."));
            }
            catch (Exception exception)
            {
                if (!OnSchedulerError(null, exception))
                    throw;
            }
        }

        private bool OnSchedulerError(IEnvelope envelope, Exception exception)
        {
            var evt = this.SchedulerItemError;
            var isHandlerAttached = (evt != null);

            if(isHandlerAttached)
                evt(this, new ScheduledItemErrorEventArgs(exception));

            return isHandlerAttached;
        }

        private void ScheduledItem_Error(object sender, ScheduledItemErrorEventArgs e)
        {
            EventHandler<ScheduledItemErrorEventArgs> evt = this.SchedulerItemError;
            if(evt != null)
                evt(this, new ScheduledItemErrorEventArgs(e.Exception));
        }

        private void ScheduledItem_Completed(object sender, ScheduledItemCompletedEventArgs e)
        {
            EventHandler<SchedulerItemCompletedEventArgs> evt = this.SchedulerItemCompleted;
            if(evt != null)
                evt(this, new SchedulerItemCompletedEventArgs(e.Message));
        }


    }
}