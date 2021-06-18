using System;

namespace DotX.Data
{
    public class KeyEventArgs : EventArgs
    {
        public int /*add enum*/ Key { get; }
        public bool IsPressed { get; }

        public KeyEventArgs(int key,
                            bool isPressed)
        {
            Key = key;
            IsPressed = isPressed;
        }
    }
}