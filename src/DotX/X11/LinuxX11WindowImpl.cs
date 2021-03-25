using System;
using DotX.Abstraction;
using Xlib = X11.Xlib;

namespace DotX.XOrg
{
    public class LinuxX11WindowImpl : IWindowImpl, IDisposable
    {
        private readonly X11.Window _window;
        private readonly LinuxX11Platform _platform;
        public LinuxX11WindowImpl(LinuxX11Platform platform)
        {
            _platform = platform;
            X11.XSetWindowAttributes attributes = new X11.XSetWindowAttributes();

            _window = Xlib.XCreateWindow(platform.Display,
                               Xlib.XDefaultRootWindow(platform.Display),
                               300,
                               300,
                               300,
                               300,
                               1,
                               0,
                               1,
                               IntPtr.Zero,
                               0,
                               ref attributes);

            Xlib.XMapWindow(_platform.Display, _window);
        }

        public void Dispose()
        {
            Xlib.XUnmapWindow(_platform.Display, _window);
            Xlib.XDestroyWindow(_platform.Display, _window);
        }
    }
}