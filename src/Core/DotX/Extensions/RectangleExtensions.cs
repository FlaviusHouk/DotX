using Cairo;
using DotX.Data;

namespace DotX.Extensions
{
    public static class RectangleExtensions
    {
        public static Rectangle Subtract(this Rectangle rect, Margin margin)
        {
            return margin.IsEmpty ? 
                rect :
                new (rect.X + margin.Left,
                     rect.Y + margin.Top,
                     rect.Width - margin.Right - margin.Left,
                     rect.Height - margin.Bottom - margin.Top);
        }

        public static Rectangle Add(this Rectangle rect, Margin margin)
        {
            return margin.IsEmpty ? 
                rect :
                new (rect.X - margin.Left,
                     rect.Y - margin.Top,
                     rect.Width + margin.Right + margin.Left,
                     rect.Height + margin.Bottom + margin.Top);
        }

        public static bool IsPointInside(this Rectangle rectangle, Point<int> p)
        {
            return rectangle.X < p.X && rectangle.X + rectangle.Width > p.X &&
                   rectangle.Y < p.Y && rectangle.Y + rectangle.Height > p.Y;
        }

        public static bool Contains (this Rectangle rectangle, Rectangle possibleChild)
        {
            return rectangle.X <= possibleChild.X &&
                   rectangle.X + rectangle.Width >= possibleChild.X + possibleChild.Width &&
                   rectangle.Y <= possibleChild.Y &&
                   rectangle.Y + rectangle.Height >= possibleChild.Y + possibleChild.Height;
        }
    }
}