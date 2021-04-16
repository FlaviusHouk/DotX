using System;
using Cairo;
using DotX.Abstraction;
using DotX.Brush;

namespace DotX.Controls
{
    public class Window : Control
    {
        private bool _isVisible;

        public IWindowImpl WindowImpl { get; }

        public Window()
        {
            Width = 300;
            Height = 300;
            Background = new SolidColorBrush(1, 0, 0);

            WindowImpl = Application.CurrentApp.Platform.CreateWindow(Width,
                                                                       Height);
            WindowImpl.Dirty += WindowDirty;
            WindowImpl.Resizing += Resizing;
        }

        private void Resizing(int width, int height)
        {
            IsMeasureDirty = true;
            Measure(new Rectangle(0,0, width, height));
            Invalidate();
        }

        public void Show()
        {
            WindowImpl.Resize(Width, Height);
            WindowImpl.Show();
            _isVisible = true;
        }

        private void WindowDirty(RenderEventArgs args)
        {
            Invalidate(new Rectangle(args.X, args.Y, args.Width, args.Height));
        }
    }
}