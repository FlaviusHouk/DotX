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
            var logger = new LoggerConfiguration().WriteTo.Console()
                                                  .CreateLogger();

            var loggerWrapper = new SerilogWrapper(logger);

            RegistrationSetup setup = new RegistrationSetup();
            setup.RegisterFixed<DotX.Interfaces.ILogger, SerilogWrapper>(new SerilogWrapper(logger));
            
            IContainer container = 
                setup.Construct(typeof(DotX.Interfaces.ILogger).Assembly,
                                typeof(SerilogWrapper).Assembly);

            Services.Initialize(new AbiocWrapper(container));

            var app = new DotX.Application(new LinuxX11Platform());

            var mainWin = new MyWindow();
            
            app.Run();
        }
    }
}
