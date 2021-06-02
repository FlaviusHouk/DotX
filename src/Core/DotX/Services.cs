using System;
using DotX.Interfaces;
using DotX.Rendering;
using DotX.Threading;

namespace DotX
{
    public static class Services
    {
        private class DummyLogger : ILogger
        {
            public void Log(string category, LogLevel level, string format, params object[] parameters)
            {}
        }

        private static Lazy<ILogger> _logger = 
            new(() => new DummyLogger());

        private static Lazy<IRenderManager> _renderManagerCreator =
            new(() => new RenderManager(Dispatcher.CurrentDispatcher));

        internal static IServiceProvider Provider { get; private set; }

        public static ILogger Logger
        {
            get
            {
                return (ILogger)Provider.GetService(typeof(ILogger)) ??
                            _logger.Value;
            }
        }

        public static ITimeline Timeline
        {
            get
            {
                return (ITimeline)Provider.GetService(typeof(ITimeline)) ??
                    DotX.Timeline.Instance;
            }
        }

        public static IRenderManager RenderManager
        {
            get
            {
                return (IRenderManager)Provider.GetService(typeof(IRenderManager)) ??
                    _renderManagerCreator.Value;
            }
        }

        public static void Initialize(IServiceProvider services)
        {
            if(Provider is not null)
                throw new InvalidOperationException();

            Provider = services;
        }
    }
}