using Cairo;
using DotX.Extensions;
using DotX.Interfaces;

namespace DotX.Widgets.Animations
{
    public abstract class AnimationBase : IAnimatable
    {
        protected AnimationBase(int period)
        {
            Period = period;
            
            Services.Timeline.Register(this);
        }

        public int Period { get; }

        public abstract void Tick();

        protected void MarkRootDirty(Visual target, Rectangle rectangle)
        {
            IRootVisual root = default;
            target.TraverseTop<Visual>(current =>
            {
                root = current as IRootVisual;
                
                return root is not null;
            });

            root?.MarkDirtyArea(rectangle);
        }
    }
}