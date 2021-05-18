using System;

namespace DotX.Abstraction
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