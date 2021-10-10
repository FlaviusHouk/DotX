using System;
using System.Collections.Generic;
using System.Linq;

namespace DotX.Xaml.Generation
{
    internal static class TypeExtensions
    {
        public static bool IsCollection(this Type t)
        {
            return t.GetInterfaces().Any(i => i.IsGenericType && 
                                              i.GetGenericTypeDefinition()  == typeof(ICollection<>));
        }
    }
}