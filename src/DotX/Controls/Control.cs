using System;
using Cairo;

namespace DotX.Controls
{
    public class Control : Widget
    {
        static Control()
        {}

        public static readonly CompositeObjectProperty ContentProperty = 
            TypedObjectProperty<Visual>.RegisterProperty<Control>(nameof(Content),
                                                                  PropertyOptions.Inherits,
                                                                  changeValueFunc: OnContentPropertyChanged);

        private static void OnContentPropertyChanged(CompositeObject arg1, Visual arg2, Visual arg3)
        {
            ((Control)arg1).OnContentChanged(arg2, arg3);
        }

        public Visual Content
        {
            get => GetValue<Visual>(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public override void Render(Context context)
        {
            base.Render(context);

            Content?.Render(context);
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
            Content?.Measure(size);

            return Content?.DesiredSize ??
                new Rectangle(size.X, size.Y, 0, 0);
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            Content?.Arrange(size);
            
            return Content?.RenderSize ??
                new Rectangle(size.X, size.Y, 0, 0);
        }
    }
}