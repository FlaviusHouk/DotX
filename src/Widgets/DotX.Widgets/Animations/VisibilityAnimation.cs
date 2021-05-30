namespace DotX.Widgets.Animations
{
    public class VisibilityAnimation : AnimationBase
    {
        private readonly Widget _widgetToAnimate;

        public VisibilityAnimation(Widget widgetToAnimate,
                                   int period) : base(period)
        {
            _widgetToAnimate = widgetToAnimate;
        }

        public override void Tick()
        {               
            MarkRootDirty(_widgetToAnimate, _widgetToAnimate.RenderSize);
            _widgetToAnimate.IsVisible = !_widgetToAnimate.IsVisible;
        }
    }
}