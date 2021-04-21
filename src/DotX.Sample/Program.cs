using System;
using DotX.Brush;
using DotX.Controls;

namespace DotX.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new DotX.Application();
            var mainWin = new DotX.Controls.Window();
            mainWin.Width = 300;
            mainWin.Height = 300;
            mainWin.Background = new SolidColorBrush(1, 0, 0);

            var stackPanel = new StackPanel();
            var textBlock = new TextBlock();
            textBlock.Foreground = new SolidColorBrush(0, 0, 0);
            textBlock.Text = "Привіт!";
            textBlock.FontFamily = "Liberation Serif";
            textBlock.FontSize = 12;
            stackPanel.AddChild(textBlock);
            mainWin.Content = stackPanel;

            var textBlock2 = new TextBlock();
            textBlock2.Foreground = new SolidColorBrush(0, 0, 0);
            textBlock2.Text = "DotX";
            textBlock2.FontFamily = "Source Code Pro Regular";
            textBlock2.FontSize = 12;
            stackPanel.AddChild(textBlock2);

            app.Windows.Add(mainWin);
            
            app.Run();
        }
    }
}
