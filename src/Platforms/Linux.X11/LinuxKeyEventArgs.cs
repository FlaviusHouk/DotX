using DotX.Data;
using X11;

namespace DotX.Platform.Linux.X
{
    internal class LinuxKeyEventArgs : KeyEventArgs
    {
        public XKeyEvent NativeEvent { get; }
        public LinuxKeyEventArgs(bool isPressed,
                                 XKeyEvent nativeEvent) : 
            base((int)nativeEvent.keycode, isPressed)
        {
            NativeEvent = nativeEvent;
        }
    }
}