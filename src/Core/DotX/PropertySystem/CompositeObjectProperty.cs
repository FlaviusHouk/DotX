using System;
using DotX.Interfaces;

namespace DotX.PropertySystem
{
    public interface IPropertyMetadata<T> : IPropertyMetadata
    {
        T DefaultValue { get; }
    }

    public class CompositeObjectProperty : IEquatable<CompositeObjectProperty>
    {
        private const PropertyOptions GeneralOptions = PropertyOptions.Inherits;
        public static IPropertyValue UnsetValue => ValueStorage.UnsetObject;

        public static CompositeObjectProperty RegisterProperty<TVal, TOwner>(string propName,
                                                                             PropertyOptions options,
                                                                             TVal defaultValue = default,
                                                                             Func<TOwner, TVal, TVal> coerceFunc = default,
                                                                             Action<TOwner, TVal, TVal> changeValueFunc = default)
            where TOwner : CompositeObject
        {
            IPropertyMetadata metadata = 
                CreateMetadata<TOwner, TVal>(options,
                                             defaultValue,
                                             coerceFunc,
                                             changeValueFunc);

            var prop = 
                new CompositeObjectProperty(propName,
                                            typeof(TVal),
                                            metadata);

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

            IPropertyMetadata newMetadata = 
                CreateMetadata<TOwner, TVal>(prop.Metadata.Options,
                                             value,
                                             coerceFunc,
                                             changeValueFunc);

            var overridenProp = 
                new CompositeObjectProperty(prop.PropName,
                                            prop.PropertyType,
                                            newMetadata);

            PropertyManager.Instance.RegisterProperty<TOwner>(overridenProp);
        }

        private static IPropertyMetadata CreateMetadata<TOwner, TVal>(PropertyOptions options,
                                                                      TVal value,
                                                                      Func<TOwner, TVal, TVal> coerceFunc = default,
                                                                      Action<TOwner, TVal, TVal> changeValueFunc = default)
            where TOwner : CompositeObject
        {
            IPropertyMetadata newMetadata;
            if((options & ~GeneralOptions) != 0)
            {
                newMetadata = 
                    new VisualPropertyMetadata<TOwner, TVal>(options,
                                                             value,
                                                             coerceFunc,
                                                             changeValueFunc);
            }
            else
            {
                newMetadata = 
                    new PropertyMetadata<TOwner, TVal>(options,
                                                       value,
                                                       coerceFunc,
                                                       changeValueFunc);
            }

            return newMetadata;
        }

        public string PropName { get; }
        public Type PropertyType { get; }
        public IPropertyMetadata Metadata { get; }

        protected CompositeObjectProperty(string propName,
                                          Type propertyType,
                                          IPropertyMetadata metadata)
        {
            PropName = propName;
            PropertyType = propertyType;
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