using System;
using System.Collections.Generic;
using System.Linq;
using DotX.Interfaces;
using DotX.Attributes;

namespace DotX.Converters
{
    [ConverterForType(typeof(IList<string>))]
    public class StringListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType)
        {
            if(value is IList<string>)
                return value;

            if(value is IEnumerable<string> enumerable)
                return enumerable.ToList();

            if(value is not string str)
                throw new Exception();

            return str.Split(';');
        }
    }
}