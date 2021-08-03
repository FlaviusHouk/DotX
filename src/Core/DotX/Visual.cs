using Cairo;
using DotX.Extensions;
using DotX.Data;

namespace DotX
{
    public abstract class Visual : CompositeObject
    {
        private Size _previousGivenSize = default;
        private Rectangle? _previousArrangeRect = default;

        protected abstract Size MeasureCore(Size size);
        protected abstract Rectangle ArrangeCore(Rectangle size);

        public Size DesiredSize { get; private set; }

        public Rectangle RenderSize { get; private set; }

        internal Rectangle DirtyArea { get; set; }

        //It will be refactored after templates will be introduced.
        public Visual VisualParent { get; set; }

        public bool IsMeasureDirty { get; internal set; }
        public bool IsArrangeDirty { get; internal set; }

        public bool IsDirty { get; internal set; }

        public void InvalidateMeasure()
        {
            if(IsMeasureDirty)
                return;

            IsMeasureDirty = true;
            Services.LayoutManager.InvalidateMeasure(this);
        }

        public virtual void Measure(Size size)
        {
            if(!IsMeasureDirty &&
               size == _previousGivenSize)
            {
               return;
            }

            DesiredSize = MeasureCore(size);
            IsMeasureDirty = false;
            _previousGivenSize = size;
        }

        public void InvalidateArrange()
        {
            if(IsArrangeDirty)
                return;

            IsArrangeDirty = true;
            Services.LayoutManager.InvalidateArrange(this);
        }

        public virtual void Arrange(Rectangle size)
        {
            if(!IsArrangeDirty &&
               _previousArrangeRect == size)
            {
                return;
            }

            RenderSize = ArrangeCore(size);
            IsArrangeDirty = false;
            _previousArrangeRect = size;
        }

        public void Invalidate(Rectangle? area = default)
        {
            InvalidateArrange();
            DirtyArea = area ?? RenderSize;

            IsDirty = true;
            Services.LayoutManager.InitiateRender(this, area);
        }

        public virtual void HitTest(HitTestResult result)
        {
            if(RenderSize.IsPointInside(result.PointToTest))
                result.AddVisual(this);
        }

        public virtual void Render(Context context)
        {
            context.Save();

            context.MoveTo(RenderSize.X, RenderSize.Y);
            OnRender(context);

            context.Rectangle(RenderSize);
            context.Clip();

            context.Restore();
        }

        protected abstract void OnRender(Context context);
    }
}