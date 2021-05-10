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
        private readonly ManualResetEventSlim _awaiter;
        
        private bool _isShuttingDown;
        private Action _waitFunc;

        private readonly PriorityQueue<DispatcherJob, OperationPriority> _queue =
            new PriorityQueue<DispatcherJob, OperationPriority>();

        private Dispatcher(Thread therad)
        {
            _therad = therad;
            _awaiter = new ManualResetEventSlim();
        }

        public void Invoke(Action action)
        {
            if(_isShuttingDown)
                return;

            var job = new DispatcherJob(action);

            if(_therad.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                _queue.Enqueue(job, OperationPriority.Normal);

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
            if(_isShuttingDown)
                return;

            _queue.Enqueue(new DispatcherJob(action), priority);

            if(!_awaiter.IsSet && _awaiter is null)
                _awaiter.Set();
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

        internal void SetWaitFunc(Action waitFunc)
        {
            _waitFunc = waitFunc;
        }

        internal void Shutdown()
        {
            //Lock?
            _isShuttingDown = true;
        }

        private void ProcessQueue(DispatcherJob job = null)
        {
            DispatcherJob current = null;
            do
            {
                if(_queue.TryDequeue(out current))
                    current.Invoke();
                else if(_isShuttingDown)
                    return;
                else if (job is null)
                    Wait();
            }
            while(!ReferenceEquals(current, job) || job == null);
        }

        private void Wait()
        {
            if(_waitFunc != null)
            {
                _waitFunc.Invoke();
                return;
            }

            _awaiter.Reset();
            _awaiter.Wait(50);
        }
    }
}