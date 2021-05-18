using System;
using System.Globalization;
using System.Linq;
using DotX.Interfaces;
using DotX.Data;
using DotX.Attributes;

namespace DotX.Converters
{
    [ConverterForType(typeof(Margin))]
    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType)
        {
            if(value is Margin m)
                return m;

            if(value is not string)
                throw new Exception();

            var parts = ((string)value).Split(';')
                                       .Select(p => double.Parse(p, NumberStyles.Float, CultureInfo.InvariantCulture))
                                       .ToArray();

            if(parts.Length != 1 && parts.Length != 2 && parts.Length != 4)
                throw new Exception();

            return parts.Length switch
            {
                1 => new Margin(parts[0]),
                2 => new Margin(parts[0], parts[1]),
                4 => new Margin(parts[0], parts[1], parts[2], parts[3])
            };
        }
    }
}