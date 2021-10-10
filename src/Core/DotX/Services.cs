using System;
using DotX.Extensions;
using DotX.Interfaces;
using DotX.Rendering;
using DotX.Threading;

namespace DotX
{
    public static class Services
    {
        public class DummyLogger : ILogger
        {
            public void Log(string category, LogLevel level, string format, params object[] parameters)
            {}
        }

        private static Lazy<IRenderManager> _renderManagerCreator =
            new(() => new DispatcherRenderManager(Dispatcher.CurrentDispatcher));

        internal static IServiceContainer Provider { get; private set; }

        public static ILogger Logger
        {
            get => Provider.GetService<ILogger>();
        }

        public static ITimeline Timeline
        {
            get
            {
                return Provider.GetService<ITimeline>() ??
                    DotX.Timeline.Instance;
            }
        }

        public static IRenderManager RenderManager
        {
            get
            {
                return Provider.GetService<IRenderManager>() ??
                    _renderManagerCreator.Value;
            }
        }
        
        public static IBackBufferFactory BackBufferFactory
        {
            get => Provider.GetService<IBackBufferFactory>();
        }

        public static IInputManager InputManager
        {
            get => Provider.GetService<IInputManager>();
        }

        public static ILayoutManager LayoutManager
        {
            get => Provider.GetService<ILayoutManager>();
        }

        public static void Initialize(IServiceContainer services)
        {
            if(Provider is not null)
                throw new InvalidOperationException();

            Provider = services;

            if(!Provider.IsRegistered<IInputManager>())
                Provider.RegisterSingleton<IInputManager, InputManager>();

            if(!Provider.IsRegistered<IBackBufferFactory>())
                Provider.RegisterSingleton<IBackBufferFactory, InMemoryBackBufferFactory>();

            if(!Provider.IsRegistered<ILogger>())
                Provider.RegisterSingleton<ILogger, DummyLogger>(); 

            if(!Provider.IsRegistered<ILayoutManager>())
                Provider.RegisterSingleton<ILayoutManager, LayoutManager>();
        }
    }
}