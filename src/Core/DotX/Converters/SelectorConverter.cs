using System;
using DotX.Interfaces;
using DotX.Brush;
using DotX.Styling;
using DotX.Attributes;

namespace DotX.Converters
{
    //TODO: make converters for multiple types.
    [ConverterForType(typeof(Selector))]
    public class SelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType)
        {
            if(!targetType.IsAssignableFrom(typeof(Selector)))
                throw new Exception();

            if(value is Selector)
                return value;

            if (value is not string str)
                throw new Exception();

            return new Selector(str);
        }
    }
}