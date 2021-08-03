using Cairo;
using DotX.Interfaces;
using System;
using Xlib = X11.Xlib;

namespace DotX.Platform.Linux.X
{
    public class LinuxX11BackBufferFactory : IBackBufferFactory
    {
        private readonly LinuxX11Platform _platform;

        public LinuxX11BackBufferFactory(IPlatform platform)
        {
            if(platform is not LinuxX11Platform linPlatform)
            {
                throw new ArgumentException();
            }

            _platform = linPlatform;
        }

        public Surface CreateBuffer(int width, int height)
        {
            var rootWindow = Application.CurrentApp.Windows[0].WindowImpl as LinuxX11WindowImpl;
            if(rootWindow is null)
            {
                throw new ArgumentException();
            }

            X11.Status status = 
                Xlib.XGetWindowAttributes(_platform.Display,
                                          rootWindow.XWindow,
                                          out var attr);

            X11.Pixmap pixmap =
                Xlib.XCreatePixmap(_platform.Display,
                                   rootWindow.XWindow,
                                   Convert.ToUInt32(width),
                                   Convert.ToUInt32(height),
                                   Convert.ToUInt32(attr.depth));

            return new XlibSurface(_platform.Display,
                                   new IntPtr((long)(ulong)pixmap),
                                   rootWindow.Visual,
                                   width,
                                   height);
        }
    }
}