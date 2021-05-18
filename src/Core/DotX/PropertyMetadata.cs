using System;
using System.Collections.Generic;
using DotX.Interfaces;

namespace DotX
{
    public class PropertyMetadata<TOwner, TValue> : IPropertyMetadata<TValue>
        where TOwner : CompositeObject 
    {
        private readonly Func<TOwner, TValue, TValue> _coerceFunc;
        private readonly Action<TOwner, TValue, TValue> _changeValueFunc;

        public TValue DefaultValue { get; }

        public PropertyMetadata(TValue defaultValue,
                                Func<TOwner, TValue, TValue> coerceFunc,
                                Action<TOwner, TValue, TValue> changeValueFunc)
        {
            DefaultValue = defaultValue;
            _coerceFunc = coerceFunc;
            _changeValueFunc = changeValueFunc;
        }

        public T Coerce<T>(CompositeObject obj, T value)
        {
            if(_coerceFunc is null)
                return value;

            if (obj is TOwner owner &&
                value is TValue val)
            {
                TValue coerced = _coerceFunc.Invoke(owner, val);

                if (coerced is T converted)
                    return converted;
            }

            throw new InvalidCastException();
        }

        public IChangeHandler InitiateChange(CompositeObject obj,
                                             CompositeObjectProperty prop)
        {
            return new ChangeHandler<TValue>(obj,
                                             obj.GetValue<TValue>(prop),
                                             this);
        }

        public void Changed<T>(CompositeObject obj, T oldVal, T newVal)
        {
            if(_changeValueFunc is null /*||
               EqualityComparer<T>.Default.Equals(oldVal, newVal)*/)
                return;

            if (obj is TOwner owner &&
                oldVal is TValue oldValue &&
                newVal is TValue newValue)
            {
                _changeValueFunc.Invoke(owner, oldValue, newValue);
                return;
            }

            throw new InvalidCastException();
        }
    }
}