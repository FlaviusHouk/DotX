using System;
using DotX.Interfaces;
using DotX.Brush;
using DotX.Attributes;

namespace DotX.Converters
{
    //TODO: make converters for multiple types.
    [ConverterForType(typeof(IBrush))]
    public class SolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType)
        {
            if(targetType.GetInterface(nameof(IBrush)) is not null)
                throw new Exception();

            if(value is IBrush)
                return value;

            if (value is not string str)
                throw new Exception();

            var parts = str.Split(';');

            //TODO: invariant culture
            if(!double.TryParse(parts[0], out var r) ||
               !double.TryParse(parts[1], out var g) ||
               !double.TryParse(parts[2], out var b))
            {
                throw new Exception();
            }

            return new SolidColorBrush(r, g, b);
        }
    }
}