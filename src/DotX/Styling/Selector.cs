using System;
using System.Collections.Generic;
using System.Linq;
using DotX.Controls;

namespace DotX.Styling
{
    public class Selector
    {
        private readonly string _typeName;
        private readonly string _name;

        private readonly List<string> _classes =
            new List<string>();

        public string RawSelector { get; }

        public Selector(string selector)
        {
            RawSelector = selector;

            int firstDot = selector.IndexOf('.');
            int firstSemicolom = selector.IndexOf(':');

            string classesString = selector.Substring(Math.Max(0, Math.Max(selector.IndexOf('.'), selector.IndexOf(':'))));

            if(classesString != selector)
            {
                selector = selector.Replace(classesString, string.Empty);
                classesString = classesString.Substring(1);
            }
            else
            {
                classesString = string.Empty;
            }

            if(selector.StartsWith("#"))
                _name = selector.Trim('#');
            else
                _typeName = selector;

            _classes.AddRange(classesString.Split(new [] {'.', ':'}, StringSplitOptions.RemoveEmptyEntries));
        }

        public bool Matches(CompositeObject obj)
        {
            if(_typeName is not null)
            {
                if(!_classes.Any() || obj is not Widget w)
                    return obj.GetType().Name == _typeName;

                return MatchClasses(w);
            }

            return false;
        }

        private bool MatchClasses(Widget w)
        {
            int i = 0, j = 0;
            foreach (var cls in w.Classes)
            {
                if (cls == _classes[i])
                {
                    if (j != 0)
                        return false;

                    i++;
                    j = 0;

                    if (i >= _classes.Count)
                        return true;

                    continue;
                }
                else if (i != 0)
                {
                    j++;
                }
            }

            return false;
        }
    }
}