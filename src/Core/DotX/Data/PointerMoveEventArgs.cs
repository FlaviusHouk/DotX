using System;

namespace DotX.Data
{
    public class PointerMoveEventArgs : EventArgs
    {
        public int X { get; }

        public int Y { get; }

        public bool IsLeaveWindow { get; }

        public PointerMoveEventArgs(int x, int y, bool isLeaveWindow)
        {
            X = x;
            Y = y;
            IsLeaveWindow = isLeaveWindow;
        }
    }
}