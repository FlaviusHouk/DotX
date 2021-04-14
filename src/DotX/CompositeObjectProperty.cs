using System;

namespace DotX
{
    public abstract class CompositeObjectProperty
    {
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

    public class TypedObjectProperty<T> : CompositeObjectProperty
    {
        public static TypedObjectProperty<T> RegisterProperty<TOwner>(string propName,
                                                                      PropertyOptions options,
                                                                      T defaultValue = default,
                                                                      Func<CompositeObject, T, T> coerceFunc = null,
                                                                      Action<CompositeObject, T, T> changeValueFunc = null)
        {
            var prop = new TypedObjectProperty<T>(propName,
                                                  typeof(T),
                                                  defaultValue,
                                                  options,
                                                  coerceFunc,
                                                  changeValueFunc);

            PropertyManager.Instance.RegisterProperty<TOwner>(prop);
            
            return prop;
        }

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
}