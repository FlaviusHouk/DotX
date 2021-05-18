using System;

namespace DotX.Abstraction
{
    public class RenderEventArgs : EventArgs
    {
        public RenderEventArgs(int x,
                               int y,
                               int width,
                               int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
    }
}