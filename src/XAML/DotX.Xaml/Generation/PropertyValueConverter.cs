using System;
using DotX.Abstraction;

namespace DotX.Xaml.Generation
{
    [ConverterForType(typeof(IPropertyValue))]
    public class PropertyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType)
        {
            if(value is IPropertyValue)
                return value;

            if(value is not string str)
                throw new Exception();

            return new StyledPropertyValue(str);
        }
    }
}