using DotX.Interfaces;

namespace DotX.Extensions
{
    public static class LogExtensions
    {
        private class LogWrapper : ILogger
        {
            private readonly ILogger _actualLogger;
            private readonly string _categoryName;

            public LogWrapper(ILogger actualLogger, 
                              string categoryName)
            {
                _actualLogger = actualLogger;
                _categoryName = categoryName;
            }

            public void Log(string category, LogLevel level, string format, params object[] parameters)
            {
                if(category != _categoryName)
                    this.LogWarning("Attempting to write messgage for category different than {0}", _categoryName);

                _actualLogger.Log(_categoryName, level, format, parameters);
            }
        }

        public static void LogTrace(this ILogger logger, string msg, params object[] parameters)
        {
            logger.Log(string.Empty, LogLevel.Trace, msg, parameters);
        }

        public static void LogDebug(this ILogger logger, string msg, params object[] parameters)
        {
            logger.Log(string.Empty, LogLevel.Debug, msg, parameters);
        }

        public static void LogInfo(this ILogger logger, string msg, params object[] parameters)
        {
            logger.Log(string.Empty, LogLevel.Info, msg, parameters);
        }

        public static void LogWarning(this ILogger logger, string msg, params object[] parameters)
        {
            logger.Log(string.Empty, LogLevel.Warning, msg, parameters);
        }

        public static void LogError(this ILogger logger, string msg, params object[] parameters)
        {
            logger.Log(string.Empty, LogLevel.Error, msg, parameters);
        }

        public static void LogCritical(this ILogger logger, string msg, params object[] parameters)
        {
            logger.Log(string.Empty, LogLevel.Critical, msg, parameters);
        }

        public static ILogger StartCategory(this ILogger logger, string categoryName)
        {
            return new LogWrapper(logger, categoryName);
        }

        public static void LogRender(this ILogger logger, string msg, params object[] parameters)
        {
#if TRACE_RENDER
            logger.Log("Rendering", LogLevel.Trace, msg, parameters);
#endif
        }

        public static void LogWindowingSystemEvent(this ILogger logger, string msg, params object[] parameters)
        {
#if TRACE_WINDOWING
            logger.Log("Windowing", LogLevel.Trace, msg, parameters);
#endif
        }

        public static void LogLayoutSystemEvent(this ILogger logger, string msg, params object[] parameters)
        {
#if TRACE_LAYOUT
            logger.Log("Layout", LogLevel.Trace, msg, parameters);
#endif
        }
    }
}