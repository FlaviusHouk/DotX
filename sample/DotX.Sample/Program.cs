using System;
using DotX.Extensions;

//Di
//Other options https://github.com/danielpalme/IocPerformance
using Abioc; //very fast resolve, but slowest possible start. 
using Abioc.Registration;
using DotX.DI.Abioc;

//Logging
using Serilog;
using DotX.Logging.Serilog;

//Platform
using DotX.Platform.Linux.X;


namespace DotX.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new LoggerConfiguration().MinimumLevel.Verbose()
                                                  .WriteTo.Console(outputTemplate: "{Timestamp:dd/MM/yy HH:mm:ss.fff}\t[{Level:u3}]\t{Message}{NewLine}{Exception}")
                                                  .CreateLogger();

            var loggerWrapper = new SerilogWrapper(logger);
            
            var diContainer = new AbiocWrapper();
            diContainer.RegisterSingleton<DotX.Interfaces.ILogger, SerilogWrapper>(loggerWrapper);
            diContainer.RegisterSingleton<DotX.Application>();

            LinuxX11Platform.Initialize(diContainer);
            Services.Initialize(diContainer);

            var app = (DotX.Application)diContainer.GetService(typeof(DotX.Application));

            var mainWin = new MyWindow();
            
            app.Run();
        }
    }
}
