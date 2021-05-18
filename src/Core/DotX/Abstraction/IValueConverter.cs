using System;

namespace DotX.Abstraction
{
    public interface IValueConverter
    {
        object Convert(object value, Type targetType);
    }
}