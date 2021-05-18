using System;
using DotX.Abstraction;

namespace DotX.Converters
{
    [ConverterForType(typeof(double))]
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType)
        {
            if(targetType != typeof(double))
                throw new Exception();

            if(value is double)
                return value;

            if (value is not string)
                throw new Exception();

            if(!double.TryParse((string)value, out var doubleValue))
                throw new Exception();

            return doubleValue;
        }
    }
}