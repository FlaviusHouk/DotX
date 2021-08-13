using System;
using System.Collections.Generic;

namespace DotX.PropertySystem
{
    public class SubscriptionHolder<T> : IDisposable
    {
        private readonly ICollection<IObserver<T>> _owner;
        private readonly IObserver<T> _observer;
        
        public SubscriptionHolder(ICollection<IObserver<T>> owner,
                                  IObserver<T> observer)
        {
            _owner = owner;
            _observer = observer;
        }
        
        public void Dispose()
        {
            Dispose(false);
        }

        private void Dispose(bool isFinalizing)
        {
            if(!isFinalizing)
                GC.SuppressFinalize(this);

            _owner.Remove(_observer);
        }

        ~SubscriptionHolder()
        {
            Dispose(true);
        }
    }
}