using System;

namespace DotX.Threading
{
    public class DispatcherJob
    {
        public event Action Completed;
        private readonly Action _action;

        public DispatcherJob(Action action)
        {
            _action = action;
        }

        public void Invoke()
        {
            _action.Invoke();
            Completed?.Invoke();
        }
    }
}