using System;

namespace DotX.Abstraction
{
    public interface IPlatform : IDisposable
    {
        event Action<WindowEventArgs> WindowCreated;

        event Action<WindowEventArgs> WindowClosed;

        IWindowImpl CreateWindow(int width, int height);
        void ListenToEvents();
    }
}