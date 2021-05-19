using System;

namespace DotX.Interfaces
{
    public interface IValueConverter
    {
        object Convert(object value, Type targetType);
    }
}