using Cairo;

namespace DotX.Interfaces
{
    public interface ILayoutManager
    {
        void InvalidateMeasure(Visual visual);

        void InvalidateArrange(Visual visual);

        void InitiateRender(Visual visual, Rectangle? area);
    }
}