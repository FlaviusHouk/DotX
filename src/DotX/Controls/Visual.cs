using Cairo;

namespace DotX.Controls
{
    public abstract class Visual : CompositeObject
    {
        public abstract void Render(Context context);

        public abstract Rectangle MeasureCore(Rectangle size);
        public abstract void ArrangeCore(Rectangle size);

        public Rectangle DesiredSize { get; private set; }

        public Rectangle RenderSize { get; private set; }

        public void Measure(Rectangle size)
        {
            DesiredSize = MeasureCore(size);
        }

        public void Arrange(Rectangle size)
        {
            RenderSize = size;
            ArrangeCore(size);
        }
    }
}