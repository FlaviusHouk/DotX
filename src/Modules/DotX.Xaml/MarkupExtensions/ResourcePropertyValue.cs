using System;
using DotX.Data;
using DotX.Interfaces;
using DotX.Extensions;
using DotX.PropertySystem;

namespace DotX.Xaml.MarkupExtensions
{
    internal class ResourcePropertyValue : Observable<IPropertyValue>,
                                           IObserver<string>,
                                           IPropertyValue
    {
        private readonly IResourceOwner _owner; 
        private IPropertyValue _cachedValue;
        private bool _isDynamic;

        private IDisposable _resourceSubscription;

        public event Action<CompositeObject, CompositeObjectProperty> Attached;
        public event Action<CompositeObject, CompositeObjectProperty> Changed;
        public event Action<CompositeObject, CompositeObjectProperty> Detached;

        public string ResourceKey { get; }
        
        public ResourcePropertyValue(string resourceKey, 
                                     IResourceOwner owner,
                                     bool isDynamic = false)
        {
            ResourceKey = resourceKey;
            _owner = owner;
            _isDynamic = isDynamic;    
        }

        public T GetValue<T>()
        {
            if(_cachedValue is not null)
                return _cachedValue.GetValue<T>();

            SetupCache<T>();
            
            return _cachedValue.GetValue<T>();
        }

        public bool Is<T>()
        {
            if(_cachedValue is not null)
                return _cachedValue.Is<T>();

            SetupCache<T>();

            return _cachedValue.Is<T>();
        }

        public T SetValue<T>(T value)
        {
            throw new NotSupportedException();
        }

        private void SetupCache<T>()
        {
            T val = default;  
             
            if(_owner.Resources.TryGetValue(ResourceKey, out var resource) &&
               resource is T)
               {
                   _cachedValue = new PropertyValue<T>((T)resource);

                   if(_isDynamic && _resourceSubscription is null)
                       _resourceSubscription = _owner.Resources.Subscribe(this);
               }

            var visualOwner = (Visual)_owner;
            visualOwner.TraverseTop<IResourceOwner>(w => 
            {
                if(w.Resources.TryGetValue(ResourceKey, out var res) &&
                   res is T)
                {
                    val = (T)res;

                    if(_isDynamic && _resourceSubscription is null)
                       _resourceSubscription = w.Resources.Subscribe(this);

                    return true;
                }
                
                return false;
            });

            _cachedValue = new PropertyValue<T>(val);
        }

        public void OnAttached(CompositeObject owner, CompositeObjectProperty prop)
        {
            Attached?.Invoke(owner, prop);
        }

        public void OnChanged(CompositeObject owner, CompositeObjectProperty prop)
        {
            Changed?.Invoke(owner, prop);
        }

        public void OnDetached(CompositeObject owner, CompositeObjectProperty prop)
        {
            Detached?.Invoke(owner, prop);
        }

        public void OnCompleted()
        {
            foreach(var observer in Observers)
                observer.OnCompleted();
        }

        public void OnError(Exception error)
        {
            foreach(var observer in Observers)
                observer.OnError(error);
        }

        public void OnNext(string value)
        {
            if(value != ResourceKey)
                return;
            
            IPropertyValue oldValue = _cachedValue;
            _cachedValue = default;
            
            foreach(var subscription in Observers)
                subscription.OnNext(oldValue);
        }
    }
}