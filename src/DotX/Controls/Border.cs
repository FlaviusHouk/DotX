using Cairo;

namespace DotX.Controls
{
    public class Border : Control
    {
        static Border()
        {}

        public static readonly CompositeObjectProperty BorderThicknessProperty =
            CompositeObjectProperty.RegisterProperty<int, Border>(nameof(BorderThickness),
                                                                  PropertyOptions.Inherits,
                                                                  1,
                                                                  (b, val) => val < 0 ? 0 : val,
                                                                  (b, oldVal, newVal) => {
                                                                      b.InvalidateMeasure();
                                                                      b.Invalidate();
                                                                  });

        public int BorderThickness
        {
            get => GetValue<int>(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        protected override Rectangle MeasureCore(Rectangle size)
        {
            Rectangle availableSize = GetContentArea(size);
            Rectangle contentSize = base.MeasureCore(availableSize);

            return new Rectangle(size.X, 
                                 size.Y, 
                                 contentSize.Width + BorderThickness * 2,
                                 contentSize.Height + BorderThickness * 2);
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            Rectangle availableSize = GetContentArea(size);
            var contentSize = base.ArrangeCore(availableSize);

            return new Rectangle(size.X, 
                                 size.Y, 
                                 contentSize.Width + BorderThickness * 2,
                                 contentSize.Height + BorderThickness * 2);
        }

        public override void Render(Context context)
        {
            base.Render(context);

            context.Save();

            Foreground.ApplyTo(context);
            context.LineWidth = BorderThickness;
            context.Rectangle(RenderSize.X + BorderThickness / 2.0,
                              RenderSize.Y + BorderThickness / 2.0,
                              RenderSize.Width - BorderThickness,
                              RenderSize.Height - BorderThickness);
            context.Stroke();
            
            context.Restore();
        }

        private Rectangle GetContentArea(Rectangle size)
        {
            double x = size.X + BorderThickness, 
                   y = size.Y + BorderThickness, 
                   width = size.Width - BorderThickness * 2, 
                   height = size.Height - BorderThickness * 2;

            if(width < 0)
                width = 0;

            if(height < 0)
                height = 0;

            return new Rectangle(x, y, width, height);
        }
    }
}