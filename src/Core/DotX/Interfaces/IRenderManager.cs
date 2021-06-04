using Cairo;

namespace DotX.Interfaces
{
    public interface IRenderManager
    {
        void Invalidate(IRootVisual root,
                        Visual visualToInvalidate,
                        Rectangle? area);

        void Expose(IRootVisual root,
                    Rectangle area);
    }
}