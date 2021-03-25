using System;
using System.Threading;

namespace DotX.Threading
{
    public class Dispatcher
    {
        private static Lazy<Dispatcher> _dispatcherCreator =
            new Lazy<Dispatcher>(() => new Dispatcher(Thread.CurrentThread));

        public static Dispatcher CurrentDispatcher => 
            _dispatcherCreator.Value;
        
        private readonly Thread _therad;

        private readonly PriorityQueue<DispatcherJob, OperationPriority> _queue =
            new PriorityQueue<DispatcherJob, OperationPriority>();

        private Dispatcher(Thread therad)
        {
            _therad = therad;
        }

        public void Invoke(Action action)
        {
            var job = new DispatcherJob(action);

            if(_therad.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                _queue.Enqueue(new DispatcherJob(action), OperationPriority.Normal);

                ProcessQueue(job);
            }
            else
            {
                ManualResetEvent ev = new ManualResetEvent(false);
                job.Completed += () => ev.Set();
                _queue.Enqueue(job, OperationPriority.Normal);
                ev.WaitOne();
            }
        }

        public void BeginInvoke(Action action, OperationPriority priority)
        {
            _queue.Enqueue(new DispatcherJob(action), priority);
        }

        public void RunLoop()
        {
            var prevContext = SynchronizationContext.Current;

            try
            {
                var newContext = new DispatcherSynchronizationContext(this);
                SynchronizationContext.SetSynchronizationContext(newContext);

                ProcessQueue();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevContext);
            }
        }

        private void ProcessQueue(DispatcherJob job = null)
        {
            DispatcherJob current = null;
            do
            {
                if(_queue.TryDequeue(out current))
                    current.Invoke();
            }
            while(!ReferenceEquals(current, job));
        }
    }
}