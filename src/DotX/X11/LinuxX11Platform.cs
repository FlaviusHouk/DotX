using System;
using DotX.Abstraction;
using DotX.Threading;
using Xlib = X11.Xlib;
using System.Runtime.InteropServices;

namespace DotX.XOrg
{
    public class LinuxX11Platform : IPlatform
    {
        public IntPtr Display { get; }
        public LinuxX11Platform()
        {
            Display = Xlib.XOpenDisplay(null);
        }

        public IWindowImpl CreateWindow()
        {
            return new LinuxX11WindowImpl(this);
        }

        public void ListenToEvents()
        {
            var d = Dispatcher.CurrentDispatcher;
            IntPtr ev = Marshal.AllocHGlobal(24 * sizeof(long));

            try
            {
                while (true)
                {
                    int eventsPendind = Xlib.XPending(Display);
                    if (eventsPendind == 0)
                    {
                        d.BeginInvoke(() => ListenToEvents(), OperationPriority.Normal);
                        break;
                    }

                    X11.Status status = Xlib.XNextEvent(Display, ev);
                    if(status == X11.Status.Failure)
                    {
                        throw new Exception();
                    }
                    var xevent = Marshal.PtrToStructure<X11.XAnyEvent>(ev);

                    HandleEvent(xevent, ev);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ev);
            }
        }

        private void HandleEvent(X11.XAnyEvent xevent, IntPtr ev)
        {
            switch(xevent.type)
            {
                default:
                    throw new NotSupportedException();
            }
        }
    }
}