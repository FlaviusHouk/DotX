using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotX.Interfaces;
using DotX.Threading;
using DotX.Data;
using DotX.Attributes;

namespace DotX
{
    public class Application
    {
        public static Application CurrentApp { get; private set; }

        public static void AddWindow(IRootVisual w)
        {
            if(CurrentApp is null)
                Dispatcher.CurrentDispatcher.BeginInvoke(() => AddWindow(w), OperationPriority.Normal);

            CurrentApp._windows.Add(w);
        }

        private readonly List<IRootVisual> _windows =
            new();

        public IPlatform Platform { get; }
        public IReadOnlyList<IRootVisual> Windows => _windows;
        
        public Application(IPlatform platform)
        {
            Platform = platform;
            CurrentApp = this;
            
            Platform.WindowClosed += OnWindowClosed;
        }

        private void OnWindowClosed(WindowEventArgs obj)
        {
            IRootVisual windowToRemove = _windows.First(w => w.WindowImpl == obj.Window);
            _windows.Remove(windowToRemove);

            //TODO: Add ApplicationLifetime class
            if(_windows.Count > 0)
                return;

            Dispatcher.CurrentDispatcher.BeginInvoke(() => {
                Platform.Dispose();
                
                Environment.Exit(0);
            }, OperationPriority.Normal);
            Dispatcher.CurrentDispatcher.Shutdown();
        }

        public void Run()
        {
            Dispatcher d = Dispatcher.CurrentDispatcher;
            d.BeginInvoke(() => _windows[0].Show(), OperationPriority.Normal);
            d.RunLoop();
        }
    }
}
