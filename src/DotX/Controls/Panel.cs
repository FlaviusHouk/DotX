using System.Collections.Generic;
using Cairo;

namespace DotX.Controls
{
    public abstract class Panel : Widget
    {
        public IList<Visual> Children { get; }
            = new List<Visual>();

        public override void Render(Context context)
        {
            base.Render(context);

            foreach(var child in Children)
            {
                context.Save();
                context.Rectangle(child.RenderSize);
                context.Clip();
                context.MoveTo(child.RenderSize.X, child.RenderSize.Y);
                
                child.Render(context);

                context.Restore();
            }
        }
    }
}