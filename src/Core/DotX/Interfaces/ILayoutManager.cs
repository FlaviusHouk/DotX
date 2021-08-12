using Cairo;

namespace DotX.Interfaces
{
    public interface ILayoutManager
    {
        void InvalidateMeasure(Visual visual);

        void InvalidateArrange(Visual visual, Rectangle prevRect);

        void InitiateRender(Visual visual, Rectangle? area);
    }
}