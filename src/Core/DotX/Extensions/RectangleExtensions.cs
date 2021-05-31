using System;
using System.Collections.Generic;
using Cairo;
using DotX.Data;

namespace DotX.Extensions
{
    //TODO:Add object pool
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

        public static double GetBottom(this Rectangle rect)
        {
            return rect.Y + rect.Height;
        }

        public static double GetRight(this Rectangle rect)
        {
            return rect.X + rect.Height;
        }

        public static bool Intersects(this Rectangle rect, Rectangle other)
        {
            return !(rect.X > other.GetRight() || 
                     rect.GetRight() < other.X || 
                     rect.Y > other.GetBottom() || 
                     rect.GetBottom() < other.Y);
        }

        public static Rectangle IntersectRect(this Rectangle rect, Rectangle other)
        {
            double x = Math.Max(rect.X, other.X);
            double y = Math.Max(rect.Y, other.Y);

            return new Rectangle(x, y,
                                 Math.Min(rect.GetRight(), other.GetRight()) - x,
                                 Math.Min(rect.GetBottom(), other.GetBottom()) - y);
        }

        public static Rectangle Union(this Rectangle rect, Rectangle other)
        {
            double x = Math.Min(rect.X, other.X);
            double y = Math.Min(rect.Y, other.Y);

            return new Rectangle(x, y,
                                 Math.Max(rect.GetRight(), other.GetRight()) - x,
                                 Math.Max(rect.GetBottom(), other.GetBottom()) - y);
        }

        public static IEnumerable<Rectangle> Subtract(this Rectangle rect, Rectangle other)
        {
            var intersect = rect.IntersectRect(other);

            if(intersect.X > rect.X)
                yield return new Rectangle(rect.X, 
                                           rect.Y, 
                                           intersect.X - rect.X, 
                                           rect.Height);

            if(intersect.Y > rect.Y)
                yield return new Rectangle(rect.X, 
                                           rect.Y, 
                                           rect.Width, 
                                           intersect.Y - rect.Y);

            if(intersect.GetBottom() < rect.GetBottom())
                yield return new Rectangle(intersect.X, 
                                           intersect.GetBottom(), 
                                           rect.Width, 
                                           rect.GetBottom() - intersect.GetBottom());

            if(intersect.GetRight() < rect.GetRight())
                yield return new Rectangle(intersect.GetRight(), 
                                           intersect.Y, 
                                           rect.GetRight() - intersect.GetRight(), 
                                           rect.Height);
        }
    }
}