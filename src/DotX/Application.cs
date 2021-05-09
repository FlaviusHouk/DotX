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
            var converters = AppDomain.CurrentDomain.GetAssemblies()
                                                    .SelectMany(ass => ass.GetTypes()
                                                                          .Where(t => t.GetInterface(nameof(IValueConverter)) is not null))
                                                    .ToDictionary(t => t.GetCustomAttribute<ConverterForTypeAttribute>().TargetType,
                                                                       t => (IValueConverter)Activator.CreateInstance(t));

            foreach(var converter in converters)
                Converters.Converters.RegisterConverter(converter.Key, converter.Value);
        }

        public void Run()
        {
            Dispatcher d = Dispatcher.CurrentDispatcher;
            d.BeginInvoke(() => _windows[0].Show(), OperationPriority.Normal);
            d.RunLoop();
        }
    }
}
