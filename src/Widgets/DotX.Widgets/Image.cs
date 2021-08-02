using Cairo;
using DotX.Data;
using DotX.Extensions;
using DotX.PropertySystem;

namespace DotX.Widgets
{
    public class Image : Widget
    {
        public static readonly CompositeObjectProperty ImageSourceProperty =
            CompositeObjectProperty.RegisterProperty<ImageSource, Image>(nameof(Source),
                                                                         PropertyOptions.Inherits);

        public ImageSource Source
        {
            get => GetValue<ImageSource>(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        protected override Size MeasureCore(Size size)
        {
            return new (Source.Width,
                        Source.Height);
        }

        protected override void OnRender(Context context)
        {
            base.OnRender(context);

            context.Rectangle(RenderSize);
            context.Image(Source);
        }
    }
}