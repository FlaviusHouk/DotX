using System;
using DotX.Platform.Linux.X;

namespace DotX.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new DotX.Application(new LinuxX11Platform());

            var mainWin = new MyWindow();
            
            app.Run();
        }
    }
}
