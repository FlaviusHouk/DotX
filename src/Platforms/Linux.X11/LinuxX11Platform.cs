using System;
using DotX.Interfaces;
using DotX.Threading;
using Xlib = X11.Xlib;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using DotX.Data;
using DotX.Extensions;

namespace DotX.Platform.Linux.X
{
    public class LinuxX11Platform : IPlatform
    {
        [DllImport("libX11.so.6")]
        public static extern X11.Atom XInternAtom(IntPtr display, string name, bool only_if_exists);


        [DllImport("libX11.so.6")]
        public static extern String XGetAtomName(IntPtr display, X11.Atom atom);

        private readonly Lazy<X11.Atom> _deleteProtocolAtomProvider;
        private X11.Atom WM_DELETE_PROTOCOL => _deleteProtocolAtomProvider.Value;

        private X11.Atom WM_PROTOCOL => XInternAtom (Display, "WM_PROTOCOLS", false);

        private readonly IList<LinuxX11WindowImpl> _windows =
            new List<LinuxX11WindowImpl>();

        public IntPtr Display { get; } 

        public event Action<WindowEventArgs> WindowCreated;

        public event Action<WindowEventArgs> WindowClosed;
        
        public LinuxX11Platform()
        {
            Display = Xlib.XOpenDisplay(null);

            //Xlib.XSetErrorHandler(OnError);

            Dispatcher.CurrentDispatcher.Initialize(ListenToEvents,
                                                    WakeUp);
                                                    
            _deleteProtocolAtomProvider =
                new Lazy<X11.Atom>(() => XInternAtom(Display, "WM_DELETE_WINDOW", false));
        }

        private void WakeUp()
        {
            Services.Logger.LogWindowingSystemEvent("Received wake up call.");

            if(!_windows.Any())
            {
                Services.Logger.LogWindowingSystemEvent("No windows to wake. Request skipped.");
                return;
            }

            var connection = Xlib.XOpenDisplay(null);

            IntPtr ev = Marshal.AllocHGlobal(24 * sizeof(long));
            try
            {
                var msg = new X11.XClientMessageEvent()
                {
                    message_type = X11.Atom.None,
                    window = _windows[0].XWindow,
                    display = connection,
                    type = (int)X11.Event.ClientMessage,
                    format = 32
                };

                Marshal.StructureToPtr(msg, ev, false);

                X11.Status status = Xlib.XSendEvent(connection, _windows[0].XWindow, true, 0, ev);
                Services.Logger.LogWindowingSystemEvent("Message to XOrg sent. Status - {0}", status);
            }
            finally
            {
                Marshal.FreeHGlobal(ev);
                Xlib.XCloseDisplay(connection);
            }
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

            Services.Logger.LogWindowingSystemEvent("Window {0} created.", wind.XWindow);

            _windows.Add(wind);
            XSetWMProtocols(wind);
            WindowCreated?.Invoke(new WindowEventArgs(wind));

            return wind;
        }

        public void ListenToEvents()
        {
            var d = Dispatcher.CurrentDispatcher;
            IntPtr ev = Marshal.AllocHGlobal(24 * sizeof(long));

            try
            {
                WaitForEvent(d, ev);

                while(Xlib.XPending(Display) > 0)
                    WaitForEvent(d, ev);
            }
            finally
            {
                Marshal.FreeHGlobal(ev);
            }
        }


        private void WaitForEvent(Dispatcher d, IntPtr ev)
        {
            Services.Logger.LogWindowingSystemEvent("Waiting for X11 events...");
            Xlib.XNextEvent(Display, ev);

            var xevent = Marshal.PtrToStructure<X11.XAnyEvent>(ev);

            HandleEvent(xevent, ev, d);
        }

        private void HandleEvent(X11.XAnyEvent xevent, IntPtr ev, Dispatcher d)
        {
            Services.Logger.LogWindowingSystemEvent("Processing event {0}...", xevent.type);

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

                case (int)X11.Event.DestroyNotify:
                    var destroyEvent = Marshal.PtrToStructure<X11.XDestroyWindowEvent>(ev);
                    HandleDestroyEvent(destroyEvent, d);
                    break;

                case (int)X11.Event.ClientMessage:
                    var clientMessage = Marshal.PtrToStructure<X11.XClientMessageEvent>(ev);
                    HandleClientMessage(clientMessage, d);
                    break;

                case (int)X11.Event.EnterNotify:
                case (int)X11.Event.LeaveNotify:
                    var crossingEvent = Marshal.PtrToStructure<X11.XCrossingEvent>(ev);
                    HandleCrossingEvent(crossingEvent, d);
                    break;

                case (int)X11.Event.MotionNotify:
                    var motionEvent = Marshal.PtrToStructure<X11.XMotionEvent>(ev);
                    HandleMoveEvent(motionEvent, d);
                    break;
            }
        }

        private void HandleMoveEvent(X11.XMotionEvent motionEvent, 
                                     Dispatcher d)
        {
            d.BeginInvoke(() => {
                var window = _windows.First(w => w.XWindow == motionEvent.window);
                var windowControl = Application.CurrentApp.Windows.First(w => w.WindowImpl == window);

                Services.Logger.LogWindowingSystemEvent("Processing motion event. Mouse position is {0}x{1}.",
                                                        motionEvent.x,
                                                        motionEvent.y);

                InputManager.Instance.DispatchPointerMove((Visual)windowControl, 
                                                          new PointerMoveEventArgs(motionEvent.x, 
                                                                                   motionEvent.y,
                                                                                   false));

            }, OperationPriority.Normal);
        }

        private void HandleCrossingEvent(X11.XCrossingEvent crossingEvent, 
                                         Dispatcher d)
        {
            d.BeginInvoke(() => {
                if(crossingEvent.type == (int)X11.Event.LeaveNotify)
                {
                    var window = _windows.First(w => w.XWindow == crossingEvent.window);
                    var windowControl = Application.CurrentApp.Windows.First(w => w.WindowImpl == window);

                    Services.Logger.LogWindowingSystemEvent("Processing crossing event. Mouse position - {0}x{1}.",
                                                            crossingEvent.x,
                                                            crossingEvent.y);

                    InputManager.Instance.DispatchPointerMove((Visual)windowControl, 
                                                              new PointerMoveEventArgs(crossingEvent.x, 
                                                                                       crossingEvent.y,
                                                                                       true));
                }
            }, OperationPriority.Normal);
        }

        private void HandleClientMessage(X11.XClientMessageEvent clientMessage, Dispatcher d)
        {
            if(clientMessage.message_type == WM_PROTOCOL)
            {
                Services.Logger.LogWindowingSystemEvent("Having client message WM_PROTOCOL.");
                //It might not work with other Window Managers then
                //Openbox. Maybe it WM_DELETE_PROTOCOL should be also
                //checked in clientMessage.data.
                d.Invoke(() => DestroyWindow(clientMessage.window));
            }
        }

        private void HandleDestroyEvent(X11.XDestroyWindowEvent destroyEvent, Dispatcher d)
        {
            Services.Logger.LogWindowingSystemEvent("Processing destroy event...");
            d.Invoke(() => DestroyWindow(destroyEvent.window));
        }

        private void HandleConfigureEvent(X11.XConfigureRequestEvent configureEvent, Dispatcher d)
        {
            Services.Logger.LogWindowingSystemEvent("Processing Configure event. Window size is {0}x{1}.",
                                                    configureEvent.width,
                                                    configureEvent.height);

            d.BeginInvoke(() => {
                var window = _windows.First(w => w.XWindow == configureEvent.window);
                window.OnResize(configureEvent.width, configureEvent.height); 
            }, OperationPriority.Normal);
        }

        private void HandleResizeEvent(X11.XResizeRequestEvent resizeEvent, Dispatcher d)
        {
            Services.Logger.LogWindowingSystemEvent("Processing resizing event. New size is {0}x{1}.",
                                                    resizeEvent.width,
                                                    resizeEvent.height);

            d.BeginInvoke(() => {
                var window = _windows.First(w => w.XWindow == resizeEvent.window);
                window.Resize(resizeEvent.width, resizeEvent.height); 
            }, OperationPriority.Normal);
        }

        private void HandleExposeEvent(X11.XExposeEvent exposeEvent, Dispatcher d)
        {
            Services.Logger.LogWindowingSystemEvent("Processing expose event. Exposed area is [{0}, {1}] - {2}x{3}.",
                                                    exposeEvent.x,
                                                    exposeEvent.y,
                                                    exposeEvent.width,
                                                    exposeEvent.height);
                   
            d.BeginInvoke(() => {
                var window = _windows.First(w => w.XWindow == exposeEvent.window);
                
                window.MarkDirty(new RenderEventArgs(exposeEvent.x,
                                                     exposeEvent.y,
                                                     exposeEvent.width,
                                                     exposeEvent.height));
            }, OperationPriority.Normal);
        }

        private void DestroyWindow(X11.Window xWindow)
        {
            Services.Logger.LogWindowingSystemEvent("Destroying window {0}.", xWindow);

            var window = _windows.First(w => w.XWindow == xWindow);
            window.Close();

            WindowClosed?.Invoke(new WindowEventArgs(window));
        }

        private void XSetWMProtocols(LinuxX11WindowImpl window)
        {
            GCHandle valueHandle = default;
            try
            {
                var atom = (ulong)WM_DELETE_PROTOCOL;
                valueHandle = GCHandle.Alloc(atom, GCHandleType.Pinned);
                
                int res = Xlib.XChangeProperty (Display, 
                                                window.XWindow, 
                                                property: WM_PROTOCOL, 
                                                type: X11.Atom.Atom, 
                                                format: 32, //bits,
		                                        (int)X11.PropertyMode.Replace, 
                                                data: valueHandle.AddrOfPinnedObject(), 
                                                nelements: 1 /*count*/);
            }
            finally
            {
                if(valueHandle.IsAllocated)
                    valueHandle.Free();
            }
        } 

        public void Dispose()
        {
            Services.Logger.LogWindowingSystemEvent("Disposing platform...");
            
            Xlib.XCloseDisplay(Display);
        }
    }
}