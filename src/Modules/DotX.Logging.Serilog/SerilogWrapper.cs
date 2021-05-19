using System;
using DotX;
using DotX.Interfaces;
using SerilogImpl = Serilog;

namespace DotX.Logging.Serilog
{
    public class SerilogWrapper : DotX.Interfaces.ILogger
    {
        private readonly SerilogImpl.ILogger _logger;
        public SerilogWrapper(SerilogImpl.ILogger logger)
        {
            _logger = logger;
        }

        public void Log(string category,
                        LogLevel level,
                        string msg,
                        params object[] parameters)
        {
            var template = msg;
            if(!string.IsNullOrEmpty(category))
                template = string.Format("[{0}]: {1}", category, msg);
            
            msg = string.Format(template, parameters);

            switch(level)
            {
                case LogLevel.Trace:
                    _logger.Verbose(msg);
                    break;
                
                case LogLevel.Debug:
                    _logger.Debug(msg);
                    break;

                case LogLevel.Info:
                    _logger.Information(msg);
                    break;

                case LogLevel.Warning:
                    _logger.Warning(msg);
                    break;

                case LogLevel.Error:
                    _logger.Error(msg);
                    break;

                case LogLevel.Critical:
                    _logger.Fatal(msg);
                    break;
            }
        }
    }
}
