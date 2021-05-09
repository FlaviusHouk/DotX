using System;
using DotX.Abstraction;

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
                return (int)value;

            if (value is not string)
                throw new Exception();

            if(!int.TryParse((string)value, out var intValue))
                throw new Exception();

            return intValue;
        }
    }
}