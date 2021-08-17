using System;
using System.Collections.Generic;
using DotX.PropertySystem;

namespace DotX.Data
{
    public abstract class Observable<T> : IObservable<T>
    {
        private List<IObserver<T>> _observers = 
            new();
            
        protected IReadOnlyCollection<IObserver<T>> Observers => _observers;
        
        protected Observable()
        {}

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if(observer is null)
                throw new ArgumentNullException(nameof(observer), "Cannot subscribe. No observer.");

            _observers.Add(observer);

            return new SubscriptionHolder<T>(_observers, observer);
        }

        protected void OnNext(T nextValue)
        {
            foreach(var observer in _observers)
                observer.OnNext(nextValue);
        }

        protected void OnError(Exception e)
        {
            foreach(var observer in _observers)
                observer.OnError(e);
        }

        protected void OnCompleted()
        {
            foreach(var observer in _observers)
                observer.OnCompleted();
        }
    }
}