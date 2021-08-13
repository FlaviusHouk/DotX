using System;
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

        public static bool TryFindChild<T>(this Widget widget,
                                           Func<T, bool> predicate, 
                                           out T child)
        {
            child = default;
            if(widget is Control control)
            {
                if(control.Child is T cand && predicate(cand))
                {
                    child = cand;
                    return true;
                }
                else if (control.Child is Widget w)
                {
                    return w.TryFindChild<T>(predicate, out child);
                }
            }
            else if(widget is Panel p)
            {
                foreach(var c in p.Children)
                {
                    if(c is T cand && predicate(cand))
                    {
                        child = cand;
                        return true;
                    }
                    else if (c is Widget w && w.TryFindChild<T>(predicate, out child))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}