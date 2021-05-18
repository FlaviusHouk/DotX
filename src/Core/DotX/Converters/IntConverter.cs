using System;
using DotX.Interfaces;
using DotX.Attributes;

namespace DotX.Converters
{
    [ConverterForType(typeof(int))]
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType)
        {
            if(targetType != typeof(int))
                throw new Exception();

            if(value is int)
                return value;

            if (value is not string)
                throw new Exception();

            if(!int.TryParse((string)value, out var intValue))
                throw new Exception();

            return intValue;
        }
    }
}