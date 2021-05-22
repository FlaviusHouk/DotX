using System;
using Cairo;
using DotX.Data;

namespace DotX.Interfaces
{
    public delegate void RenderRequest(RenderEventArgs args);
    public delegate void ResizingDelegate (int width, int height);
    public interface IWindowImpl
    {
        void Show();

        void Resize(int width, int height);

        void UpdateBackground(IBrush brush);

        void Close();
        
        Surface WindowSurface { get; }

        event RenderRequest Dirty;
        event ResizingDelegate Resizing;

        event Action Closed;
        
    }
}