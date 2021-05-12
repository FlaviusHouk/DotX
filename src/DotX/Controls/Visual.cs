using Cairo;
using DotX.Abstraction;
using DotX.Extensions;

namespace DotX.Controls
{
    public abstract class Visual : CompositeObject
    {
        public abstract void Render(Context context);

        protected abstract Rectangle MeasureCore(Rectangle size);
        protected abstract Rectangle ArrangeCore(Rectangle size);

        public Rectangle DesiredSize { get; private set; }

        public Rectangle RenderSize { get; private set; }

        internal Rectangle DirtyArea { get; set; }

        public Visual VisualParent { get; internal set; }

        public bool IsMeasureDirty { get; internal set; } = true;
        public bool IsArrangeDirty { get; internal set; } = true;

        public bool IsDirty { get; internal set; }

        public void InvalidateMeasure()
        {
            if(!IsMeasureDirty)
                return;

            LayoutManager.Instance.InvalidateMeasure(this);
        }

        public void Measure(Rectangle size)
        {
            DesiredSize = MeasureCore(size);
            IsMeasureDirty = false;
        }

        public void InvalidateArrange()
        {
            if(!IsArrangeDirty)
                return;

            LayoutManager.Instance.InvalidateArrange(this);
        }

        public void Arrange(Rectangle size)
        {
            RenderSize = ArrangeCore(size);
            IsArrangeDirty = false;
        }

        public void Invalidate(Rectangle? area = default)
        {
            InvalidateArrange();
            DirtyArea = area ?? RenderSize;

            IsDirty = true;
            LayoutManager.Instance.InitiateRender(this, area);
        }

        public virtual void HitTest(HitTestResult result)
        {
            if(RenderSize.IsPointInside(result.PointToTest))
                result.AddVisual(this);
        }
    }
}