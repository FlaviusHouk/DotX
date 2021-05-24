using System;

namespace DotX.PropertySystem
{
    public class PropertyValue<T> : PropertyValueBase
    {
        private T _value;

        public PropertyValue(T value)
        {
            _value = value;
        }

        public override TVal GetValue<TVal>()
        {
            if (_value is TVal val)
                return val;
            else if (_value is null)
                return default;

            throw new InvalidCastException();
        }

        public override bool Is<TVal>()
        {
            return _value is TVal;
        }

        public override TVal SetValue<TVal>(TVal value)
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