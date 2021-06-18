using System;

namespace DotX.Data
{
    public class PointerMoveEventArgs : EventArgsWithCoords
    {
        public bool IsLeaveWindow { get; }

        public PointerMoveEventArgs(int x, int y, bool isLeaveWindow) :
            base(x, y)
        {
            IsLeaveWindow = isLeaveWindow;
        }
    }
}