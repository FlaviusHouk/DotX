using System;
using System.Reflection;

namespace DotX.Xaml
{
    internal class XamlProperty
    {
        public string PropertyName { get; }
        public string RawValue { get; }

        public Type PropertyType { get; private set; }

        public XamlProperty(string propName, string rawValue)
        {
            if (string.IsNullOrEmpty(propName))
                throw new ArgumentException($"'{nameof(propName)}' cannot be null or empty.", nameof(propName));

            if (string.IsNullOrEmpty(rawValue))
                throw new ArgumentException($"'{nameof(rawValue)}' cannot be null or empty.", nameof(rawValue));

            PropertyName = propName;
            RawValue = rawValue;
        }

        public void Invalidate(Type owner)
        {
            var prop = owner.GetProperty(PropertyName, BindingFlags.Public | BindingFlags.Instance);
            
            if(prop is null)
                throw new Exception();

            PropertyType = prop.PropertyType;
        }
    }
}