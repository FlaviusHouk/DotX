using System;

namespace DotX
{
    public abstract class CompositeObjectProperty
    {
        public static CompositeObjectProperty RegisterProperty<TVal, TOwner>(string propName,
                                                                             PropertyOptions options,
                                                                             TVal defaultValue = default,
                                                                             Func<TOwner, TVal, TVal> coerceFunc = default,
                                                                             Action<TOwner, TVal, TVal> changeValueFunc = default)
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

        public static void OverrideProperty<TVal, TOwner>(CompositeObjectProperty prop,
                                                          TVal value,
                                                          Func<TOwner, TVal, TVal> coerceFunc = default,
                                                          Action<TOwner, TVal, TVal> changeValueFunc = default)
            where TOwner : CompositeObject
        {
            if(prop is not TypedObjectProperty<TVal>)
                throw new InvalidOperationException("Cannot override property type.");

            var overridenProp = new TypedObjectProperty<TVal, TOwner>(prop.PropName,
                                                                      value,
                                                                      prop.Options,
                                                                      coerceFunc,
                                                                      changeValueFunc);

            PropertyManager.Instance.RegisterProperty<TOwner>(overridenProp);
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

    internal abstract class TypedObjectProperty<T> : CompositeObjectProperty
    {
        public T DefaultValue { get; }

        protected TypedObjectProperty(string propName,
                                      Type propertyType,
                                      T defaultValue,
                                      PropertyOptions options) : 
            base(propName, propertyType, options)
        {
            DefaultValue = defaultValue;
        }

        public abstract T Coerce(CompositeObject obj, T value);

        public abstract void Changed(CompositeObject obj, T oldValue, T newValue);
    }

    internal class TypedObjectProperty<TVal, TOwner> : TypedObjectProperty<TVal>
        where TOwner : CompositeObject
    {
        private readonly Func<TOwner, TVal, TVal> _coerceFunc;
        private readonly Action<TOwner, TVal, TVal> _changeValueFunc;
        public TypedObjectProperty(string propName,
                                   TVal defaultValue, 
                                   PropertyOptions options, 
                                   Func<TOwner, TVal, TVal> coerceFunc = null, 
                                   Action<TOwner, TVal, TVal> changeValueFunc = null) : 
            base(propName, typeof(TOwner), defaultValue, options)
        {
            _coerceFunc = coerceFunc;
            _changeValueFunc = changeValueFunc;
        }

        public override TVal Coerce(CompositeObject obj, TVal value)
        {
            return _coerceFunc is null ? value : _coerceFunc((TOwner)obj, value);
        }

        public override void Changed(CompositeObject obj, TVal oldValue, TVal newValue)
        {
            _changeValueFunc?.Invoke((TOwner)obj, oldValue, newValue);
        }
    }
}