using Cairo;
using DotX.Threading;

namespace DotX.Controls
{
    public abstract class Visual : CompositeObject
    {
        public abstract void Render(Context context);

        protected abstract Rectangle MeasureCore(Rectangle size);
        protected abstract void ArrangeCore(Rectangle size);

        public Rectangle DesiredSize { get; private set; }

        public Rectangle RenderSize { get; private set; }

        internal Rectangle DirtyArea { get; set; }

        public Visual VisualParent { get; internal set; }

        public bool IsMeasureDirty { get; internal set; }
        public bool IsArrangeDirty { get; internal set; }

        public bool IsDirty { get; internal set; }

        public void InvalidateMeasure()
        {
            LayoutManager.Instance.InvalidateMeasure(this);
        }

        public void Measure(Rectangle size)
        {
            if(!IsMeasureDirty)
                return;

            DesiredSize = MeasureCore(size);
            IsMeasureDirty = false;
        }

        public void InvalidateArrange()
        {
            LayoutManager.Instance.InvalidateArrange(this);
        }

        public void Arrange(Rectangle size)
        {
            if(!IsArrangeDirty)
                return;

            RenderSize = size;
            ArrangeCore(size);
            IsArrangeDirty = false;
        }

        public void Invalidate(Rectangle? area = default)
        {
            InvalidateArrange();
            DirtyArea = area ?? RenderSize;

            IsDirty = true;
            Dispatcher.CurrentDispatcher.BeginInvoke(() => LayoutManager.Instance.InitiateRender(this), 
                                                     OperationPriority.Normal);
        }
    }
}