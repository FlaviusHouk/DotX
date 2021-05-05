using System;
using System.Collections.Generic;
using Cairo;
using DotX.Abstraction;

namespace DotX.Controls
{
    public abstract class Panel : Widget
    {
        private readonly List<Visual> _children = new();

        [ContentProperty]
        public IReadOnlyList<Visual> Children => _children;

        public void AddChild(Visual child)
        {
            _children.Add(child);
            
            if(child is Widget w)
            {
                if(w.LogicalParent is not null)
                    throw new InvalidOperationException("Cannot add child. Already has a parent.");

                w.LogicalParent = this;
            }

            child.VisualParent = this; //while there are no templates it is ok.
        }

        public void RemoveChild(Visual child)
        {
            _children.Remove(child);

            if(child is Widget w)
                w.LogicalParent = default;

            child.VisualParent = default; //while there are no templates it is ok.
        }

        public override void Render(Context context)
        {
            if(!IsVisible)
                return;

            base.Render(context);

            foreach(var child in Children)
            {
                if(child is Widget widget && !widget.IsVisible)
                    continue;

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