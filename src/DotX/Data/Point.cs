using System;

namespace DotX.Data
{
    public abstract class Point : IEquatable<Point>
    {
        public abstract bool Equals(Point other);
    }

    public class Point<T> : Point
    {
        public T X { get; }
        public T Y { get; }

        public Point(T x, T y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj != null &&
                   obj is Point point &&
                   Equals(point);
        }

        public override bool Equals(Point other)
        {
            return other is Point<T> point &&
                   X.Equals(point.X) && 
                   Y.Equals(point.Y);  
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}