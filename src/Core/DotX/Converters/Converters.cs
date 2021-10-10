using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using DotX.Attributes;
using DotX.Interfaces;

namespace DotX.Converters
{
    public static class Converters
    {
        static Converters()
        {
            var converters = AppDomain.CurrentDomain.GetAssemblies()
                                                    .SelectMany(ass => ass.GetTypes()
                                                                          .Where(t => t.GetInterface(nameof(IValueConverter)) is not null))
                                                    .ToDictionary(t => t.GetCustomAttribute<ConverterForTypeAttribute>().TargetType,
                                                                       t => (IValueConverter)Activator.CreateInstance(t));

            foreach(var converter in converters)
                RegisterConverter(converter.Key, converter.Value);
        }

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