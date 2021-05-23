using System;
using DotX.Attributes;
using DotX.Interfaces;

namespace DotX.Converters
{
    [ConverterForType(typeof(ImageSource))]
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType)
        {
            if(value is ImageSource)
                return value;

            if(value is not string str)
                throw new Exception();

            return new CairoImageSource(str);
        }
    }
}