using Cairo;
using DotX.Extensions;

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

        protected override Rectangle MeasureCore(Rectangle size)
        {
            return new Rectangle(size.X,
                                 size.Y,
                                 Source.Width,
                                 Source.Height);
        }

        public override void Render(Context context)
        {
            base.Render(context);

            context.Rectangle(RenderSize);
            context.Image(Source);
        }
    }
}