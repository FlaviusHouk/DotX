using System.Threading;

namespace DotX.Threading
{
    public class DispatcherSynchronizationContext : SynchronizationContext
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherSynchronizationContext(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            _dispatcher.Invoke(() => d.Invoke(state));
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _dispatcher.BeginInvoke(() => d.Invoke(state), OperationPriority.Normal);
        }
    }
}