using System;

namespace DotX.Data
{
    public abstract class EventArgsWithCoords : EventArgs
    {
        public int X { get; }

        public int Y { get; }

        protected EventArgsWithCoords(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}