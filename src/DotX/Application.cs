using System;
using System.Collections.Generic;
using DotX.Abstraction;
using DotX.Controls;
using DotX.Threading;
using DotX.XOrg;

namespace DotX
{
    public class Application
    {
        public static Application CurrentApp { get; private set; }
        private static IPlatform GetPlatform()
        {
            return new LinuxX11Platform();
        }

        private readonly List<Window> _windows =
            new List<Window>();

        public IPlatform Platform { get; }
        public IList<Window> Windows => _windows;
        public Application()
        {
            Platform = GetPlatform();
            CurrentApp = this;
        }

        public void Run()
        {
            Dispatcher d = Dispatcher.CurrentDispatcher;
            d.BeginInvoke(() => Platform.ListenToEvents(), OperationPriority.Normal);
            d.RunLoop();
        }
    }
}
