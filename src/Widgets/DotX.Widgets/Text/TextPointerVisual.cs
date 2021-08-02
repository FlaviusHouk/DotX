using Cairo;
using DotX.Extensions;
using DotX.PropertySystem;

namespace DotX.Widgets.Text
{
    internal class TextPointerVisual : Widget
    {
        static TextPointerVisual()
        {
            var metadata = new VisualPropertyMetadata<TextPointerVisual, bool>(PropertyOptions.AffectsParentRender,
                                                                               false);
                                                                               
            CompositeObjectProperty.OverrideProperty<TextPointerVisual>(IsVisibleProperty,
                                                                        metadata);
        }
        public TextPointerVisual()
        {
            Width = 1;
        }

        protected override void OnRender(Context context)
        {
            context.Rectangle(RenderSize);
            Foreground.ApplyTo(context);
            context.Fill();
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            return DesiredSize.ToRectangle(size.X, size.Y);
        }
    }
}