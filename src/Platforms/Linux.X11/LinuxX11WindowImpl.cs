using System;
using System.Runtime.InteropServices;
using Cairo;
using DotX.Interfaces;
using Xlib = X11.Xlib;
using DotX.Data;
using DotX.Extensions;
using DotX.Brush;

namespace DotX.Platform.Linux.X
{
    public class LinuxX11WindowImpl : IWindowImpl, IDisposable
    {
        private bool _isClosing;
        private readonly X11.Window _window;
        private readonly IntPtr _visual;
        private readonly LinuxX11Platform _platform;

        private XlibSurface _cairoSurface;

        public event RenderRequest Dirty;
        public event ResizingDelegate Resizing;
        public event Action Closed;

        public X11.Window XWindow => _window;

        public Surface WindowSurface
        {
            get
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

                return _cairoSurface;
            }
        }

        public LinuxX11WindowImpl(LinuxX11Platform platform,
                                  int width,
                                  int height)
        {
            _platform = platform;
            X11.XSetWindowAttributes attributes = new X11.XSetWindowAttributes();
            var screen = Xlib.XDefaultScreen(_platform.Display);
            _visual = Xlib.XDefaultVisual(_platform.Display, screen);
            var depth = Xlib.XDefaultDepth(_platform.Display, screen);
            attributes.background_pixel = 0;
            attributes.bit_gravity = 10;

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
                               1<<1 | 1<<4,
                               ref attributes);

            Xlib.XSelectInput(platform.Display, _window, X11.EventMask.ExposureMask | 
                                                         X11.EventMask.StructureNotifyMask |
                                                         X11.EventMask.EnterWindowMask | 
                                                         X11.EventMask.LeaveWindowMask |
                                                         X11.EventMask.PointerMotionMask);
        }

        public void Show()
        {
            Xlib.XMapWindow(_platform.Display, _window);
        }

        public void MarkDirty(RenderEventArgs args)
        {
            Dirty?.Invoke(args);
        }
        
        public void OnResize(int width, int height)
        {
            if(_cairoSurface is not null)
            {
                var oldHeight = _cairoSurface.Height;
                var oldWidth = _cairoSurface.Width;

                if(oldWidth == width &&
                   oldHeight == height)
                {
                    Services.Logger.LogWindowingSystemEvent("Having resize request, but size didn't change. Ignoring.");
                    return;
                }

                _cairoSurface.SetSize(width, height);
            }

            Resizing?.Invoke(width, height);
        }

        public void UpdateBackground(IBrush brush)
        {
            if(brush is not SolidColorBrush solidColor)
                return;

            X11.XColor color = new X11.XColor();
            var colormap = Xlib.XDefaultColormap(_platform.Display,
                                                 Xlib.XDefaultScreen(_platform.Display));

            var status = Xlib.XParseColor(_platform.Display,
                                          colormap,
                                          $"RGBi:{solidColor.Red}/{solidColor.Green}/{solidColor.Blue}",
                                          ref color);

            //Not sure if XLib.XFreeColors should be called here.
            //The API is different and XColor is allocated by the .Net
            //It is probably should be called to remove Colormap entry
            status = Xlib.XAllocColor(_platform.Display, colormap, ref color);

            Xlib.XSetWindowBackground(_platform.Display,
                                      XWindow,
                                      color.pixel);
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

        public void Close()
        {
            if(_isClosing)
                return;

            _isClosing = true;

            Dispose();

            Closed?.Invoke();
        }
    }
}