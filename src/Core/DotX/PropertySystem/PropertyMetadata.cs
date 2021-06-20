using System;
using System.Collections.Generic;

namespace DotX.PropertySystem
{
    public class PropertyMetadata<TOwner, TValue> : IPropertyMetadata<TValue>
        where TOwner : CompositeObject 
    {
        private readonly Func<TOwner, TValue, TValue> _coerceFunc;
        private readonly Action<TOwner, TValue, TValue> _changeValueFunc;

        public TValue DefaultValue { get; }

        public PropertyOptions Options { get; }

        public PropertyMetadata(PropertyOptions options,
                                TValue defaultValue,
                                Func<TOwner, TValue, TValue> coerceFunc = default,
                                Action<TOwner, TValue, TValue> changeValueFunc = default)
        {
            Options = options;
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

        public virtual void Changed<T>(CompositeObject obj, T oldVal, T newVal)
        {
            if(_changeValueFunc is null ||
               EqualityComparer<T>.Default.Equals(oldVal, newVal))
                return;

            if (obj is TOwner owner)
            {
                //It was fun but it should be simplified...
                TValue oldValue = GetValue<TValue, T>(oldVal);
                TValue newValue = GetValue<TValue, T>(newVal);

                _changeValueFunc.Invoke(owner, oldValue, newValue);
                return;
            }

            throw new InvalidCastException();
        }

        private TResult GetValue<TResult, TValue>(TValue value)
        {
            if(value is TResult result)
                return result;
            else if(value is null)
                return default;
            
            throw new InvalidCastException();
        }
    }
}