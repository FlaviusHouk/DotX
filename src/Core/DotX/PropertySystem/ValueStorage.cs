using System;
using System.Collections.Generic;
using System.Linq;
using DotX.Interfaces;

namespace DotX.PropertySystem
{
    internal class ValueStorage
    {
        private static Lazy<ValueStorage> _storage = new Lazy<ValueStorage>(() => new ValueStorage());

        public static ValueStorage Storage => _storage.Value;

        public static IPropertyValue UnsetObject => UnsetValue.Value;

        private class UnsetValue : PropertyValueBase
        {
            public static readonly Lazy<UnsetValue> _value = 
                new Lazy<UnsetValue>(() => new UnsetValue());

            public static UnsetValue Value => _value.Value;

            private UnsetValue()
            {}

            public override T GetValue<T>()
            {
                throw new NotSupportedException();
            }

            public override T SetValue<T>(T value)
            {
                throw new NotSupportedException();
            }

            public override bool Is<T>() => false;
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

        public IPropertyValue GetValue(CompositeObject owner,
                                       CompositeObjectProperty prop)
        {
            return _valueStorage[owner][prop];
        }

        public void SetValue(CompositeObject owner,
                             CompositeObjectProperty prop,
                             IPropertyValue value)
        {
            Dictionary<CompositeObjectProperty, IPropertyValue> props;
            if(!_valueStorage.TryGetValue(owner, out props))
            {
                props = new Dictionary<CompositeObjectProperty, IPropertyValue>();
                _valueStorage.Add(owner, props);
            }

            var oldValue = props[prop];
            props[prop] = value;
            oldValue.OnDetached(owner, prop);
            value.OnAttached(owner, prop);
        }

        public bool IsSet(CompositeObject obj, 
                          CompositeObjectProperty prop)
        {
            return _valueStorage[obj][prop] is not UnsetValue;
        }
    }
}