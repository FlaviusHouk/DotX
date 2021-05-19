using System;
using DotX.Interfaces;

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

        internal static IServiceProvider Provider { get; private set; }

        public static ILogger Logger
        {
            get
            {
                return (ILogger)Provider.GetService(typeof(ILogger)) ??
                            _logger.Value;
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