using System;
using System.Collections.Generic;
using DotX.Abstraction;

namespace DotX.Converters
{
    public static class Converters
    {
        private static Dictionary<Type, IValueConverter> _converters = 
            new Dictionary<Type, IValueConverter>();

        public static IEnumerable<IValueConverter> AvailableConverters { get; }

        public static IValueConverter GetConverterForType(Type targetType)
        {
            return _converters[targetType];
        }

        public static bool TryGetConverterForType(Type targetType, out IValueConverter converter)
        {
            return _converters.TryGetValue(targetType, out converter);
        }

        public static void RegisterConverter(Type target, IValueConverter converter)
        {
            _converters.Add(target, converter);
        }
    }
}