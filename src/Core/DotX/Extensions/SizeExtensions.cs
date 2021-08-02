using Cairo;
using DotX.Data;

namespace DotX.Extensions
{
    public static class SizeExtensions
    {
        public static Rectangle ToRectangle(this Size size,
                                            double x,
                                            double y)
        {
            return new Rectangle(x, y, size.Width, size.Height);
        }

        public static Size Subtract(this Size size, Margin margin)
        {
            return margin.IsEmpty ? 
                size :
                new (size.Width - margin.Right - margin.Left,
                     size.Height - margin.Bottom - margin.Top);
        }

        public static Size Add(this Size size, Margin margin)
        {
            return margin.IsEmpty ? 
                size :
                new (size.Width + margin.Right + margin.Left,
                     size.Height + margin.Bottom + margin.Top);
        }
    }
}