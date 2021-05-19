using System;
using DotX.Interfaces;

namespace DotX.Data
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