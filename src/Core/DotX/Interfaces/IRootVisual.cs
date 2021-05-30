using Cairo;

namespace DotX.Interfaces
{
    public interface IRootVisual
    {
        IWindowImpl WindowImpl { get; }

        bool IsVisible { get; }

        //TODO: Add posibility to set
        //with RenderManager after render.
        Rectangle? DirtyArea { get; }

        void Show();

        void MarkDirtyArea(Rectangle area);

        void CleanDirtyArea();
    }
}