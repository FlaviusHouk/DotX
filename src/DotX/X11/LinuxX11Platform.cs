using System;
using DotX.Abstraction;
using DotX.Threading;
using Xlib = X11.Xlib;
using System.Runtime.InteropServices;
using Mono.Unix.Native;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace DotX.XOrg
{
    public class LinuxX11Platform : IPlatform
    {
        private readonly List<LinuxX11WindowImpl> _windows = 
            new List<LinuxX11WindowImpl>();

        public IntPtr Display { get; } 
        
        public LinuxX11Platform()
        {
            Display = Xlib.XOpenDisplay(null);

            Xlib.XSetErrorHandler(OnError);
            Dispatcher.CurrentDispatcher.SetWaitFunc(ListenToEvents);
        }

        private int OnError(IntPtr display, ref X11.XErrorEvent ev)
        {
            int size = 256;
            var mem = Marshal.AllocHGlobal(sizeof(char) * size);

            var status = Xlib.XGetErrorText(display, 
                                            ev.error_code, 
                                            mem,
                                            size);

            string msg = Marshal.PtrToStringAuto(mem);

            return 0;
        }

        public IWindowImpl CreateWindow(int width, int height)
        {
             
            var wind = new LinuxX11WindowImpl(this, 
                                              width == 0 ? 1 : width, 
                                              height == 0 ? 1 : height);

            _windows.Add(wind);
            return wind;
        }

        public void ListenToEvents()
        {
            var d = Dispatcher.CurrentDispatcher;
            IntPtr ev = Marshal.AllocHGlobal(24 * sizeof(long));

            try
            {
                Xlib.XNextEvent(Display, ev);

                var xevent = Marshal.PtrToStructure<X11.XAnyEvent>(ev);

                HandleEvent(xevent, ev, d);
            }
            finally
            {
                Marshal.FreeHGlobal(ev);
            }
        }

        private void HandleEvent(X11.XAnyEvent xevent, IntPtr ev, Dispatcher d)
        {
            switch(xevent.type)
            {
                case (int)X11.Event.Expose:
                    var exposeEvent = Marshal.PtrToStructure<X11.XExposeEvent>(ev);
                    HandleExposeEvent(exposeEvent, d);
                    break;
                
                case (int)X11.Event.ResizeRequest:
                    while (Xlib.XCheckMaskEvent(Display, X11.EventMask.ResizeRedirectMask, ev)) 
                    {}

                    var resizeEvent = Marshal.PtrToStructure<X11.XResizeRequestEvent>(ev);
                    HandleResizeEvent(resizeEvent, d);
                    break;
                
                case (int)X11.Event.ConfigureNotify:
                    var configuraEvent = Marshal.PtrToStructure<X11.XConfigureRequestEvent>(ev);
                    HandleConfigureEvent(configuraEvent, d);
                    break;
            }
        }

        private void HandleConfigureEvent(X11.XConfigureRequestEvent configuraEvent, Dispatcher d)
        {
            d.Invoke(() => {
                var window = _windows.First(w => w.XWindow == configuraEvent.window);
                window.OnResize(configuraEvent.width, configuraEvent.height); 
            });
        }

        private void HandleResizeEvent(X11.XResizeRequestEvent resizeEvent, Dispatcher d)
        {
            d.Invoke(() => {
                var window = _windows.First(w => w.XWindow == resizeEvent.window);
                window.Resize(resizeEvent.width, resizeEvent.height); 
            });
        }

        private void HandleExposeEvent(X11.XExposeEvent exposeEvent, Dispatcher d)
        {
            d.Invoke(() => {
                var window = _windows.First(w => w.XWindow == exposeEvent.window);
                
                window.MarkDirty(new RenderEventArgs(exposeEvent.x,
                                                     exposeEvent.y,
                                                     exposeEvent.width,
                                                     exposeEvent.height));
            });
        }
    }
}