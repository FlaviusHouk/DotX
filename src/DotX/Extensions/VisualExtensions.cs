using System;
using DotX.Controls;

namespace DotX.Extensions
{
    public static class VisualExtensions
    {
        public static void TraverseTop<T>(this Visual current, Func<T, bool> traverser)
            where T : Visual
        {
            current = current.VisualParent;
            while(current is not null)
            {
                if(current is T obj && traverser(obj))
                    break;

                current = current.VisualParent;
            }
        }
    }
}