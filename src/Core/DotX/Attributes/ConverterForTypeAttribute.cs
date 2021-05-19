using System;

namespace DotX.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConverterForTypeAttribute : Attribute
    {
        public Type TargetType { get; }

        public ConverterForTypeAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}