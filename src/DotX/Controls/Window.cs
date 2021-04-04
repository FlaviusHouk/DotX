using System;
using Cairo;
using DotX.Abstraction;

namespace DotX.Controls
{
    public class Window
    {
        private int _width = 300;
        private int _height = 300;
        private bool _isVisible;

        public int Width 
        {
            get => _width;
            set
            {
                if(_width != value)
                {
                    _width = value;

                    if(_isVisible)
                        _windowImpl.Resize(_width, _height);
                }
            }
        }

        private readonly IWindowImpl _windowImpl;
        public Window()
        {
            _windowImpl = Application.CurrentApp.Platform.CreateWindow(_width,
                                                                       _height);
            _windowImpl.Dirty += WindowDirty;
            _windowImpl.Resizing += Resizing;
        }

        private void Resizing(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Show()
        {
            _windowImpl.Resize(_width, _height);
            _windowImpl.Show();
            _isVisible = true;
        }

        private void Render(Context content)
        {
            content.Rectangle(0, 0, Width, _height);
            content.SetSourceRGB(1, 0, 0);
            content.Fill();
        }

        private void WindowDirty(Context context, RenderEventArgs args)
        {
            Render(context);
        }
    }
}