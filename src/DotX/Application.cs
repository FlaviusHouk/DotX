using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotX.Abstraction;
using DotX.Controls;
using DotX.Threading;
using DotX.XOrg;

namespace DotX
{
    public class Application
    {
        public static Application CurrentApp { get; private set; }

        public static void AddWindow(Window w)
        {
            if(CurrentApp is null)
                Dispatcher.CurrentDispatcher.BeginInvoke(() => AddWindow(w), OperationPriority.Normal);

            CurrentApp._windows.Add(w);
        }

        private static IPlatform GetPlatform()
        {
            return new LinuxX11Platform();
        }

        private static void RegisterConverters()
        {
            var converters = AppDomain.CurrentDomain.GetAssemblies()
                                                    .SelectMany(ass => ass.GetTypes()
                                                                          .Where(t => t.GetInterface(nameof(IValueConverter)) is not null))
                                                    .ToDictionary(t => t.GetCustomAttribute<ConverterForTypeAttribute>().TargetType,
                                                                       t => (IValueConverter)Activator.CreateInstance(t));

            foreach(var converter in converters)
                Converters.Converters.RegisterConverter(converter.Key, converter.Value);
        }

        private readonly List<Window> _windows =
            new List<Window>();

        public IPlatform Platform { get; }
        public IReadOnlyList<Window> Windows => _windows;
        public Application()
        {
            Platform = GetPlatform();
            CurrentApp = this;
            
            RegisterConverters();
            
            Platform.WindowClosed += OnWindowClosed;
        }

        private void OnWindowClosed(WindowEventArgs obj)
        {
            Window windowToRemove = _windows.First(w => w.WindowImpl == obj.Window);
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
