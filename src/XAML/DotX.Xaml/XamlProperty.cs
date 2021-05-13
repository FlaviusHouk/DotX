using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotX.Xaml
{
    internal class XamlProperty
    {
        public string PropertyName { get; }

        public Type PropertyType { get; private set; }

        public XamlProperty(string propName)
        {
            if (string.IsNullOrEmpty(propName))
                throw new ArgumentException($"'{nameof(propName)}' cannot be null or empty.", nameof(propName));

            PropertyName = propName;
        }

        public void Invalidate(Type owner)
        {
            var prop = owner.GetProperty(PropertyName, BindingFlags.Public | BindingFlags.Instance);
            
            if(prop is null)
                throw new Exception();

            PropertyType = prop.PropertyType;
        }
    }

    internal class InlineXamlProperty : XamlProperty
    {
        public string RawValue { get; }

        public InlineXamlProperty(string propName, string rawValue) :
            base(propName)
        {
            if (string.IsNullOrEmpty(rawValue))
                throw new ArgumentException($"'{nameof(rawValue)}' cannot be null or empty.", nameof(rawValue));

            RawValue = rawValue;
        }
    }

    internal class FullXamlProperty : XamlProperty
    {
        private readonly List<XamlObject> _children =
            new List<XamlObject>();
        public IReadOnlyCollection<XamlObject> Children => _children;

        public FullXamlProperty(string propName) : 
            base(propName)
        {}

        public void AddChild(XamlObject child)
        {
            _children.Add(child);
        }
    }

    internal class ExtendedXamlProperty : XamlProperty
    {
        public XamlObject Extension;

        public ExtendedXamlProperty(string propName, XamlObject extension) :
            base(propName)
        {
            Extension = extension ??
                throw new ArgumentNullException(nameof(extension));
        }
    }
}