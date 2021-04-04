using System;
using Cairo;
using DotX.Abstraction;
using Xlib = X11.Xlib;

namespace DotX.XOrg
{
    public class LinuxX11WindowImpl : IWindowImpl, IDisposable
    {
        private readonly X11.Window _window;
        private readonly IntPtr _visual;
        private readonly LinuxX11Platform _platform;

        private XlibSurface _cairoSurface;
        private Context _context;

        public event RenderRequest Dirty;
        public event ResizingDelegate Resizing;

        public X11.Window XWindow => _window;

        public LinuxX11WindowImpl(LinuxX11Platform platform,
                                  int width,
                                  int height)
        {
            _platform = platform;
            X11.XSetWindowAttributes attributes = new X11.XSetWindowAttributes();
            var screen = Xlib.XDefaultScreen(_platform.Display);
            _visual = Xlib.XDefaultVisual(_platform.Display, screen);
            var depth = Xlib.XDefaultDepth(_platform.Display, screen);
            attributes.background_pixel = Xlib.XWhitePixel(_platform.Display, screen);

            _window = Xlib.XCreateWindow(platform.Display,
                               Xlib.XDefaultRootWindow(platform.Display),
                               300,
                               300,
                               (uint)width,
                               (uint)height,
                               1,
                               depth,
                               1,
                               _visual,
                               1<<1,
                               ref attributes);

            Xlib.XSelectInput(platform.Display, _window, X11.EventMask.ExposureMask | 
                                                         X11.EventMask.StructureNotifyMask);
        }

        public void Show()
        {
            Xlib.XMapWindow(_platform.Display, _window);
        }

        public void MarkDirty(RenderEventArgs args)
        {
            using Context context = CreateContext();
            Dirty?.Invoke(context, args);
        }
        
        public void OnResize(int width, int height)
        {
            if(_cairoSurface != null)
            {
                var oldHeight = _cairoSurface.Height;
                var oldWidth = _cairoSurface.Width;

                _cairoSurface.SetSize(width, height);
            }

            Resizing?.Invoke(width, height);

            MarkDirty(new RenderEventArgs(0, 0, width, height));
        }

        public Context CreateContext()
        {
            if (_cairoSurface is null)
            {
                Xlib.XGetWindowAttributes(_platform.Display, 
                                          _window, 
                                          out var attr);

                _cairoSurface = new XlibSurface(_platform.Display,
                                                new IntPtr((long)(ulong)_window),
                                                _visual,
                                                (int)attr.width,
                                                (int)attr.height);
            }

            return new Context(_cairoSurface);
        }

        public void Dispose()
        {
            _cairoSurface.Dispose();

            Xlib.XUnmapWindow(_platform.Display, _window);
            Xlib.XDestroyWindow(_platform.Display, _window);
        }

        public void Resize(int width, int height)
        {
            var conf = new X11.XWindowChanges()
            {
                width = width,
                height = height
            };

            Xlib.XConfigureWindow(_platform.Display,
                                  _window,
                                  1<<2 | 1<<3,
                                  ref conf);
        }
    }
}