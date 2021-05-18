using System;
using DotX.Data;

namespace DotX.Interfaces
{
    public interface IPlatform : IDisposable
    {
        event Action<WindowEventArgs> WindowCreated;

        event Action<WindowEventArgs> WindowClosed;

        IWindowImpl CreateWindow(int width, int height);
        void ListenToEvents();
    }
}