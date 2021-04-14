using System;
using System.Collections.Generic;
using System.Linq;

namespace DotX
{
    internal class PropertyManager
    {
        private static Lazy<PropertyManager> _instance = 
            new Lazy<PropertyManager>(() => new PropertyManager());

        public static PropertyManager Instance => _instance.Value;

        private readonly Dictionary<Type, IList<CompositeObjectProperty>> _registeredProperties =
            new Dictionary<Type, IList<CompositeObjectProperty>>();

        private PropertyManager()
        {}

        public IEnumerable<CompositeObjectProperty> GetProperties(Type objType)
        {
            return _registeredProperties.TryGetValue(objType, out var props) ? 
                props : Enumerable.Empty<CompositeObjectProperty>();
        }

        public void RegisterProperty<TOwner>(CompositeObjectProperty property)
        {
            var t = typeof(TOwner);

            if(_registeredProperties.TryGetValue(t, out var list))
            {
                list.Add(property);
            }
            else
            {
                list = new List<CompositeObjectProperty>()
                {
                    property
                };
                
                _registeredProperties.Add(t, list);
            }
        }

        public bool IsPropertyAvailable(Type tOwner, CompositeObjectProperty property)
        {
            bool available = _registeredProperties.TryGetValue(tOwner, out var props) &&
                props.Contains(property);

            if(property.Options.HasFlag(PropertyOptions.Inherits))
            {
                var t = tOwner.BaseType;
                while(!available && t is not null)
                {
                    available = _registeredProperties.TryGetValue(t, out props) &&
                        props.Contains(property);

                    t = t.BaseType;
                }
            }

            return available;
        }
    }
}