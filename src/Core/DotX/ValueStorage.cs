using System;
using System.Collections.Generic;
using System.Linq;
using DotX.Abstraction;
using DotX.Controls;

namespace DotX
{
    internal class ValueStorage
    {
        private static Lazy<ValueStorage> _storage = new Lazy<ValueStorage>(() => new ValueStorage());

        public static ValueStorage Storage => _storage.Value;

        public static IPropertyValue UnsetObject => UnsetValue.Value;

        private class UnsetValue : IPropertyValue
        {
            public static readonly Lazy<UnsetValue> _value = 
                new Lazy<UnsetValue>(() => new UnsetValue());

            public static UnsetValue Value => _value.Value;

            private UnsetValue()
            {}

            public T GetValue<T>()
            {
                throw new NotSupportedException();
            }

            public T SetValue<T>(T value)
            {
                throw new NotSupportedException();
            }

            public bool Is<T>() => false;
        }

        private readonly Dictionary<CompositeObject, Dictionary<CompositeObjectProperty, IPropertyValue>> _valueStorage =
            new Dictionary<CompositeObject, Dictionary<CompositeObjectProperty, IPropertyValue>>();
        
        private ValueStorage()
        {}

        public void Init(CompositeObject owner, IEnumerable<CompositeObjectProperty> props)
        {
            _valueStorage.Add(owner, props.ToDictionary(p => p, 
                                                        p => (IPropertyValue)UnsetValue.Value));
        }

        public T GetValue<T>(CompositeObject owner,
                             CompositeObjectProperty prop)
        {
            var propValue = _valueStorage[owner][prop];
            
            if(typeof(T) == typeof(IPropertyValue) &&
               propValue is T val)
               return val;

            if(propValue.Is<T>())
                return propValue.GetValue<T>();
            else if (prop.Metadata is IPropertyMetadata<T> typedMetadata)
                return typedMetadata.DefaultValue;
            else
                return default;
        }

        public void SetValue<T>(CompositeObject owner,
                                CompositeObjectProperty prop,
                                T value)
        {
            Dictionary<CompositeObjectProperty, IPropertyValue> props;
            if(!_valueStorage.TryGetValue(owner, out props))
            {
                props = new Dictionary<CompositeObjectProperty, IPropertyValue>();
                _valueStorage.Add(owner, props);
            }

            if(value is IPropertyValue val)
            {
                IChangeHandler handler = prop.Metadata.InitiateChange(owner, prop);
                props[prop] = val;

                //Don't like this solution. Should be refactored.
                //Reason for it that we cannot get Resource value
                //until visual tree is ready.
                if((owner is IInitializable w && w.IsInitialized) ||
                   owner is not IInitializable)
                {
                    handler.Changed(val);
                }

                return;
            }

            if(!props.TryGetValue(prop, out var propValue))
            {
                props.Add(prop, new PropertyValue<T>(value));
                return;
            }

            value = prop.Metadata.Coerce(owner, value);

            if(propValue is UnsetValue)
            {
                propValue = new PropertyValue<T>(value);
                props[prop] = propValue;
            }

            T oldValue = propValue.SetValue(value);

            prop.Metadata.Changed(owner, oldValue, value);
        }

        public bool IsSet(CompositeObject obj, 
                          CompositeObjectProperty prop)
        {
            return _valueStorage[obj][prop] is not UnsetValue;
        }
    }
}