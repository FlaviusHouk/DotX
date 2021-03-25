using DotX.Abstraction;

namespace DotX.Controls
{
    public class Window
    {
        private readonly IWindowImpl _windowImpl;
        public Window()
        {
            _windowImpl = Application.CurrentApp.Platform.CreateWindow();
        }
    }
}