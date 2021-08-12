using Cairo;
using DotX.Data;
using DotX.PropertySystem;
using DotX.Extensions;

namespace DotX.Widgets
{
    public class Border : Control
    {
        static Border()
        {}

        public static readonly CompositeObjectProperty BorderThicknessProperty =
            CompositeObjectProperty.RegisterProperty<int, Border>(nameof(BorderThickness),
                                                                  PropertyOptions.Inherits | 
                                                                  PropertyOptions.AffectsMeaure | 
                                                                  PropertyOptions.AffectsArrange | 
                                                                  PropertyOptions.AffectsRender,
                                                                  defaultValue: 1,
                                                                  coerceFunc: (b, val) => val < 0 ? 0 : val);

        public int BorderThickness
        {
            get => GetValue<int>(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        protected override Size MeasureCore(Size size)
        {
            Size availableSize = GetContentArea(size);
            Size contentSize = base.MeasureCore(availableSize);

            return new (contentSize.Width + BorderThickness * 2,
                        contentSize.Height + BorderThickness * 2);
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            Logger.LogLayoutSystemEvent("Arranging {0}. Having x - {1}, y - {2}, w - {3}, h - {4}",
                                        NameToLog,
                                        size.X,
                                        size.Y,
                                        size.Width,
                                        size.Height);

            Rectangle availableSize = GetContentArea(size);
            var contentSize = base.ArrangeCore(availableSize);

            return new Rectangle(size.X, 
                                 size.Y, 
                                 contentSize.Width + BorderThickness * 2,
                                 contentSize.Height + BorderThickness * 2);
        }

        protected override void OnRender(Context context)
        {
            Logger.LogLayoutSystemEvent("Rendering {0}. Having x - {1}, y - {2}, w - {3}, h - {4}",
                                        NameToLog,
                                        RenderSize.X,
                                        RenderSize.Y,
                                        RenderSize.Width,
                                        RenderSize.Height);

            context.MoveTo(RenderSize.X + BorderThickness / 2.0,
                           RenderSize.Y + BorderThickness / 2.0);
                           
            base.OnRender(context);

            Foreground.ApplyTo(context);
            context.LineWidth = BorderThickness;
            context.Rectangle(RenderSize.X + BorderThickness / 2.0,
                              RenderSize.Y + BorderThickness / 2.0,
                              RenderSize.Width - BorderThickness,
                              RenderSize.Height - BorderThickness);
            context.Stroke();
        }

        private Size GetContentArea(Size size)
        {
            double width = size.Width - BorderThickness * 2, 
                   height = size.Height - BorderThickness * 2;

            if(width < 0)
                width = 0;

            if(height < 0)
                height = 0;

            return new (width, height);
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

            return new (x, y, width, height);
        }
    }
}