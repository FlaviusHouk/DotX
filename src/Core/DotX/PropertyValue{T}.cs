using System;
using DotX.Interfaces;

namespace DotX
{
    public class PropertyValue<T> : IPropertyValue
    {
        private T _value;

        public PropertyValue(T value)
        {
            _value = value;
        }

        public TVal GetValue<TVal>()
        {
            if (_value is TVal val)
                return val;

            throw new InvalidCastException();
        }

        public bool Is<TVal>()
        {
            return _value is TVal;
        }

        public TVal SetValue<TVal>(TVal value)
        {
            if (value is not T val)
                throw new InvalidCastException();

            T oldValue = _value;

            _value = val;

            if (oldValue is not TVal old)
                throw new InvalidCastException();

            return old;
        }
    }
}