using System;

namespace DotX.Abstraction
{
    public class WindowEventArgs : EventArgs
    {
        public IWindowImpl Window { get; }

        public WindowEventArgs(IWindowImpl window)
        {
            Window = window;
        }
    }
}