using System;

namespace DotX.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new DotX.Application();
            var mainWin = new DotX.Controls.Window();
            app.Windows.Add(mainWin);
            
            app.Run();
        }
    }
}
