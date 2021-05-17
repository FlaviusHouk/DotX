using System;
using DotX.Brush;
using DotX.Controls;
using DotX.Xaml;

namespace DotX.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new DotX.Application();

            var mainWin = new MyWindow();
            
            app.Run();
        }
    }
}
