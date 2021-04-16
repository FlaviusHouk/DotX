using System.Collections.Generic;

namespace DotX.Controls
{
    public abstract class Panel : Widget
    {
        public IList<Visual> Children { get; }
            = new List<Visual>();
    }
}