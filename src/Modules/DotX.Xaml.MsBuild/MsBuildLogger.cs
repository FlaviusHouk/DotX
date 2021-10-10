using DotX.Interfaces;
using Microsoft.Build.Utilities;

namespace DotX.Xaml.MsBuild
{
    internal class MsBuildLogger : ILogger
    {
        private readonly TaskLoggingHelper _logger;

        public MsBuildLogger(TaskLoggingHelper msBuildLogger)
        {
            _logger = msBuildLogger;
        }
        public void Log(string category, LogLevel level, string format, params object[] parameters)
        {
            string msg = string.Format(format, parameters);
            if(!string.IsNullOrEmpty(category))
                msg = string.Format("[{0}]: {1}", category, msg);

            switch (level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    _logger.LogError(msg);
                    break;
                
                case LogLevel.Warning:
                    _logger.LogWarning(msg);
                    break;

                case LogLevel.Info:
                case LogLevel.Debug:
                case LogLevel.Trace:
                    _logger.LogMessage(msg);
                    break;
            }
        }
    }
}