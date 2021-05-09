using System;
using Cairo;
using DotX.Abstraction;
using DotX.Extensions;

namespace DotX.Controls
{
    public class Control : Widget
    {
        static Control()
        {}

        public static readonly CompositeObjectProperty ContentProperty = 
            CompositeObjectProperty.RegisterProperty<Visual, Control>(nameof(Content),
                                                                      PropertyOptions.Inherits,
                                                                      changeValueFunc: OnContentPropertyChanged);

        private static void OnContentPropertyChanged(Control control, Visual oldValue, Visual newValue)
        {
            control.OnContentChanged(oldValue, newValue);
        }

        [ContentProperty]
        public Visual Content
        {
            get => GetValue<Visual>(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public override void Render(Context context)
        {
            base.Render(context);

            if (!IsVisible || Content is null)
                return;

            Content.Render(context);
        }

        private void OnContentChanged(Visual oldValue, Visual newValue)
        {
            if(oldValue is not null)
            {
                oldValue.VisualParent = null;

                if(oldValue is Widget oldWidget)
                    oldWidget.LogicalParent = null;
            }

            if(newValue?.VisualParent is not null)
                throw new Exception("Already have a parent");

            newValue.VisualParent = this;

            if(newValue is Widget newWidget)
            {
                newWidget.LogicalParent = this;
                newWidget.ApplyStyles();
            }

            InvalidateMeasure();
            Invalidate();
        }

        protected override Rectangle MeasureCore(Rectangle size)
        {
            if(!IsVisible || Content is null)
                return new Rectangle(size.X, size.Y, 0, 0);

            Rectangle adjustedSize = size.Subtract(Padding);
            Widget w = default;
            if(Content is Widget)
            {
                w = (Widget)Content;
                adjustedSize = adjustedSize.Subtract(w.Margin);
            }

            Content.Measure(adjustedSize);

            Rectangle desiredSize = Content.DesiredSize.Add(Padding);
            if(w is not null)
                desiredSize = desiredSize.Add(w.Margin);

            return desiredSize;
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            if(!IsVisible || Content is null)
                return new Rectangle(size.X, size.Y, 0, 0);

            Rectangle adjustedSize = size.Subtract(Padding);
            Widget w = default;
            if(Content is Widget)
            {
                w = (Widget)Content;
                adjustedSize = adjustedSize.Subtract(w.Margin);
            }

            Content.Arrange(adjustedSize);

            Rectangle renderSize = Content.RenderSize.Add(Padding);
            if(w is not null)
                renderSize = renderSize.Add(w.Margin);
            
            return renderSize;
        }

        protected override void ApplyStylesForChildren()
        {
            (Content as Widget)?.ApplyStyles();
        }
    }
}