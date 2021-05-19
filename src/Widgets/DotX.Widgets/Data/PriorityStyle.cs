using System;
using DotX.Styling;

namespace DotX.Widgets.Data
{
    internal class PriorityStyle : IComparable
    {
        public Style Style { get; }
        public int Priority { get; }

        public PriorityStyle(Style style, int priority)
        {
            Style = style;
            Priority = priority;
        }

        public int CompareTo(object obj)
        {
            if (obj is not PriorityStyle p)
                throw new ArgumentException();

            return Priority - p.Priority;
        }
    }
}