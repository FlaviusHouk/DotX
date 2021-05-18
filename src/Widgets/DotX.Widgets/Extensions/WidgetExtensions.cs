using System.Collections.Generic;
using System.Linq;
using DotX;
using DotX.Widgets.Data;
using DotX.Extensions;

namespace DotX.Widgets.Extensions
{
    public static class WidgetExtensions
    {
        internal static IReadOnlyCollection<PriorityStyle> GetStylesForElement(this Widget owner,
                                                                               CompositeObject obj)
        {
            int i = 0;
            var styles = owner.Styles.Where(s => s.Selector.Matches(obj))
                                     .Select(s => new PriorityStyle(s, i));

            owner.TraverseTop<Widget>(w =>
            {
                i++;

                styles = styles.Concat(w.Styles.Where(s => s.Selector.Matches(obj))
                                               .Select(s => new PriorityStyle(s, i)));

                return false;
            });

            return styles.ToArray();
        }
    }
}