using Cairo;

namespace DotX.Abstraction
{
    public delegate void RenderRequest(RenderEventArgs args);
    public delegate void ResizingDelegate (int width, int height);
    public interface IWindowImpl
    {
        void Show();

        void Resize(int width, int height);
        
        Surface WindowSurface { get; }

        event RenderRequest Dirty;
        event ResizingDelegate Resizing;
        
    }
}