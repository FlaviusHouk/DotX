using System;
using DotX.Interfaces;
using DotX;
using DotX.Extensions;

namespace DotX.Xaml.MarkupExtensions
{
    internal class ResourcePropertyValue : IPropertyValue
    {
        private readonly IResourceOwner _owner; 
        private IPropertyValue _cachedValue;
        public string ResourceKey { get; }
        
        public ResourcePropertyValue(string resourceKey, IResourceOwner owner)
        {
            ResourceKey = resourceKey;
            _owner = owner;
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
               }

            var visualOwner = (Visual)_owner;
            visualOwner.TraverseTop<IResourceOwner>(w => {
                if(w.Resources.TryGetValue(ResourceKey, out var res) &&
                   res is T)
                {
                    val = (T)res;
                    return true;
                }
                
                return false;
            });

            _cachedValue = new PropertyValue<T>(val);
        }
    }
}