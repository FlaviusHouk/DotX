using System;

namespace DotX
{
    public abstract class CompositeObjectProperty
    {
        public static CompositeObjectProperty RegisterProperty<TVal, TOwner>(string propName,
                                                                             PropertyOptions options,
                                                                             TVal defaultValue = default,
                                                                             Func<TOwner, TVal, TVal> coerceFunc = null,
                                                                             Action<TOwner, TVal, TVal> changeValueFunc = null)
            where TOwner : CompositeObject
        {
            var prop = new TypedObjectProperty<TVal, TOwner>(propName,
                                                             defaultValue,
                                                             options,
                                                             coerceFunc,
                                                             changeValueFunc);

            PropertyManager.Instance.RegisterProperty<TOwner>(prop);
            
            return prop;
        }

        public string PropName { get; }
        public Type PropertyType { get; }
        public PropertyOptions Options { get; }

        protected CompositeObjectProperty(string propName,
                                          Type propertyType,
                                          PropertyOptions options)
        {
            PropName = propName;
            PropertyType = propertyType;
            Options = options;
        }
    }

    [Flags]
    public enum PropertyOptions
    {
        Inherits = 1,
    }

    internal class TypedObjectProperty<T> : CompositeObjectProperty
    {
        private readonly Func<CompositeObject, T, T> _coerceFunc;
        private readonly Action<CompositeObject, T, T> _changeValueFunc;
        public T DefaultValue { get; }

        protected TypedObjectProperty(string propName,
                                      Type propertyType,
                                      T defaultValue,
                                      PropertyOptions options,
                                      Func<CompositeObject, T, T> coerceFunc = null,
                                      Action<CompositeObject, T, T> changeValueFunc = null) : 
            base(propName, propertyType, options)
        {
            DefaultValue = defaultValue;
            _coerceFunc = coerceFunc;
            _changeValueFunc = changeValueFunc;
        }

        public T Coerce(CompositeObject obj, T value)
        {
            return _coerceFunc is null ? value : _coerceFunc(obj, value);
        }

        public void Changed(CompositeObject obj, T oldValue, T newValue)
        {
            _changeValueFunc?.Invoke(obj, oldValue, newValue);
        }
    }

    internal class TypedObjectProperty<TVal, TOwner> : TypedObjectProperty<TVal>
        where TOwner : CompositeObject
    {
        public TypedObjectProperty(string propName,
                                   TVal defaultValue, 
                                   PropertyOptions options, 
                                   Func<TOwner, TVal, TVal> coerceFunc = null, 
                                   Action<TOwner, TVal, TVal> changeValueFunc = null) : 
            base(propName, typeof(TOwner), defaultValue, options, 
                 (o, v) => coerceFunc is null ? v : coerceFunc.Invoke((TOwner)o, v), 
                 (o, ov, nv) => changeValueFunc?.Invoke((TOwner)o, ov, nv))
        {
        }
    }
}