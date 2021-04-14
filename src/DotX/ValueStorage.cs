using System;
using System.Collections.Generic;
using System.Linq;

namespace DotX
{
    internal class ValueStorage
    {
        private static Lazy<ValueStorage> _storage = new Lazy<ValueStorage>(() => new ValueStorage());

        public static ValueStorage Storage => _storage.Value;

        private interface IPropertyValue 
        {}

        private class UnsetValue : IPropertyValue
        {
            public static readonly Lazy<UnsetValue> _value = 
                new Lazy<UnsetValue>(() => new UnsetValue());

            public static UnsetValue Value => _value.Value;

            private UnsetValue()
            {}
        }
        

        private interface IPropertyValue<T> : IPropertyValue
        {
            T GetValue();    

            T SetValue(T value);        
        }

        private class PropertyValue<T> : IPropertyValue<T>
        {
            private T _value;

            public PropertyValue(T value)
            {
                _value = value;
            }

            public T GetValue()
            {
                return _value;
            }

            public T SetValue(T value)
            {
                var oldVal = _value;
                _value = value;
                return oldVal;
            }
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
            
            if(propValue is IPropertyValue<T> typedValue)
                return typedValue.GetValue();
            else if (prop is TypedObjectProperty<T> typedProp)
                return typedProp.DefaultValue;
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

            if(!props.TryGetValue(prop, out var propValue))
            {
                props.Add(prop, new PropertyValue<T>(value));
                return;
            }

            var typedProp = prop as TypedObjectProperty<T>;
            if(typedProp is not null)
                value = typedProp.Coerce(owner, value);

            T oldValue = default;
            if(prop is IPropertyValue<T> typedValue)
                oldValue = typedValue.SetValue(value);
            else
                props[prop] = new PropertyValue<T>(value);

            typedProp?.Changed(owner, oldValue, value);
        }
    }
}