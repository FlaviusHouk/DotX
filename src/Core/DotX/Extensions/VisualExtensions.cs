using System;
using DotX;

namespace DotX.Extensions
{
    public static class VisualExtensions
    {
        public static bool TraverseTop<T>(this Visual current, Func<T, bool> traverser)
            where T : class
        {
            current = current.VisualParent;
            while(current is not null)
            {
                if(current is T obj && traverser(obj))
                    return true;

                current = current.VisualParent;
            }

            return false;
        }
    }
}