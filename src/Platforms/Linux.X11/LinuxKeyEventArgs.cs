using DotX.Data;
using X11;

namespace DotX.Platform.Linux.X
{
    internal class LinuxKeyEventArgs : KeyEventArgs
    {
        public XKeyEvent NativeEvent { get; }
        public LinuxKeyEventArgs(int key,
                                 bool isPressed,
                                 XKeyEvent nativeEvent) : 
            base(key, isPressed)
        {
            NativeEvent = nativeEvent;
        }
    }
}