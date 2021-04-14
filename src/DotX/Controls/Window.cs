using System;
using Cairo;
using DotX.Abstraction;

namespace DotX.Controls
{
    public class Window : Control
    {
        private bool _isVisible;

        private readonly IWindowImpl _windowImpl;
        public Window()
        {
            Width = 300;
            Height = 300;

            _windowImpl = Application.CurrentApp.Platform.CreateWindow(Width,
                                                                       Height);
            _windowImpl.Dirty += WindowDirty;
            _windowImpl.Resizing += Resizing;
        }

        private void Resizing(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void Show()
        {
            _windowImpl.Resize(Width, Height);
            _windowImpl.Show();
            _isVisible = true;
        }

        public override void Render(Context content)
        {
            content.Rectangle(0, 0, Width, Height);
            content.SetSourceRGB(1, 0, 0);
            content.Fill();
        }

        private void WindowDirty(Context context, RenderEventArgs args)
        {
            Render(context);
        }
    }
}