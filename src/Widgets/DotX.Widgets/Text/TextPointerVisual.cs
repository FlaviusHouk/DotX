using Cairo;

namespace DotX.Widgets.Text
{
    internal class TextPointerVisual : Widget
    {
        public TextPointerVisual()
        {
            Width = 1;
        }

        public override void Render(Context context)
        {
            if(!IsVisible)
                return;

            context.Rectangle(RenderSize);
            Foreground.ApplyTo(context);
            context.Fill();
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            return DesiredSize;
        }
    }
}