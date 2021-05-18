using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using DotX.Interfaces;
using DotX.Data;
using DotX.Attributes;

namespace DotX.Widgets
{
    public abstract class Panel : Widget
    {
        private readonly List<Visual> _children = new();

        [ContentProperty]
        public IReadOnlyList<Visual> Children => _children;

        public void AddChild(Visual child)
        {
            _children.Add(child);
            
            child.VisualParent = this; //while there are no templates it is ok.)

            if(child is Widget w)
            {
                if(w.LogicalParent is not null)
                    throw new InvalidOperationException("Cannot add child. Already has a parent.");

                w.LogicalParent = this;
                w.ApplyStyles();
            }
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

        protected override void ApplyStylesForChildren()
        {
            foreach(var child in Children.OfType<Widget>())
                child.ApplyStyles();
        }

        public override void HitTest(HitTestResult result)
        {
            foreach(var child in Children)
            {
                child.HitTest(result);
            }

            base.HitTest(result);
        }

        protected override void OnInitialize()
        {
            foreach(var child in Children.OfType<Widget>())
                child.Initialize();
        }
    }
}