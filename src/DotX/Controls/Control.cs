using System;
using Cairo;
using DotX.Abstraction;

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
                newWidget.LogicalParent = null;

            InvalidateMeasure();
            Invalidate();
        }

        protected override Rectangle MeasureCore(Rectangle size)
        {
            if(!IsVisible || Content is null)
                new Rectangle(size.X, size.Y, 0, 0);

            Content.Measure(size);

            return Content.DesiredSize;
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            if(!IsVisible || Content is null)
                new Rectangle(size.X, size.Y, 0, 0);

            Content.Arrange(size);
            
            return Content.RenderSize;
        }
    }
}