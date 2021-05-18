using System;
using DotX.Abstraction;

namespace DotX
{
    public interface IPropertyMetadata<T> : IPropertyMetadata
    {
        T DefaultValue { get; }
    }

    internal class ChangeHandler<TValue> : IChangeHandler
    {
        private readonly IPropertyMetadata _metadata;
        private readonly CompositeObject _obj;
        private readonly TValue _oldValue;

        public ChangeHandler(CompositeObject obj,
                             TValue oldValue,
                             IPropertyMetadata metadata)
        {
            _obj = obj;
            _oldValue = oldValue;
            _metadata = metadata;
        }

        public void Changed(IPropertyValue value)
        {
            _metadata.Changed<TValue>(_obj, _oldValue, value.GetValue<TValue>());
        }
    }

    public class CompositeObjectProperty : IEquatable<CompositeObjectProperty>
    {
        public static IPropertyValue UnsetValue => ValueStorage.UnsetObject;

        public static CompositeObjectProperty RegisterProperty<TVal, TOwner>(string propName,
                                                                             PropertyOptions options,
                                                                             TVal defaultValue = default,
                                                                             Func<TOwner, TVal, TVal> coerceFunc = default,
                                                                             Action<TOwner, TVal, TVal> changeValueFunc = default)
            where TOwner : CompositeObject
        {
            var prop = 
                new CompositeObjectProperty(propName,
                                            typeof(TVal),
                                            options,
                                            new PropertyMetadata<TOwner, TVal>(defaultValue,
                                                                               coerceFunc,
                                                                               changeValueFunc));

            PropertyManager.Instance.RegisterProperty<TOwner>(prop);
            
            return prop;
        }

        public static void OverrideProperty<TVal, TOwner>(CompositeObjectProperty prop,
                                                          TVal value,
                                                          Func<TOwner, TVal, TVal> coerceFunc = default,
                                                          Action<TOwner, TVal, TVal> changeValueFunc = default)
            where TOwner : CompositeObject
        {
            if(prop.Metadata is not IPropertyMetadata<TVal>)
                throw new InvalidOperationException("Cannot override property type.");

            var overridenProp = 
                new CompositeObjectProperty(prop.PropName,
                                            prop.PropertyType,
                                            prop.Options,
                                            new PropertyMetadata<TOwner, TVal>(value,
                                                                               coerceFunc,
                                                                               changeValueFunc));

            PropertyManager.Instance.RegisterProperty<TOwner>(overridenProp);
        }

        public string PropName { get; }
        public Type PropertyType { get; }
        public PropertyOptions Options { get; }
        public IPropertyMetadata Metadata { get; }

        protected CompositeObjectProperty(string propName,
                                          Type propertyType,
                                          PropertyOptions options,
                                          IPropertyMetadata metadata)
        {
            PropName = propName;
            PropertyType = propertyType;
            Options = options;
            Metadata = metadata;
        }

        public override bool Equals(object obj)
        {
            return obj is CompositeObjectProperty prop && Equals(prop); 
        }

        public bool Equals(CompositeObjectProperty other)
        {
            return PropName == other.PropName && 
                   PropertyType == other.PropertyType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PropName, PropertyType);
        }

        public override string ToString()
        {
            return string.Format("Name {0} for type {1}", PropName, PropertyType);
        }
    }
}