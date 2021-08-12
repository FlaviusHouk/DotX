namespace DotX.Data
{
    public record Margin
    {
        public double Left { get; }

        public double Top { get; }

        public double Right { get; }

        public double Bottom { get; }

        public bool IsEmpty => Left == 0 &&
                               Right == 0 &&
                               Top == 0 &&
                               Bottom == 0;

        public Margin()
        {
            Top = Left = Bottom = Right = 0;
        }

        public Margin(double value)
        {
            Left = Top = Right = Bottom = value;
        }

        public Margin(double horizontal,
                      double vertical)
        {
            Left = Right = horizontal;
            Top = Bottom = vertical;
        }

        public Margin(double left,
                      double top,
                      double right,
                      double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public static Margin operator+(Margin one, Margin second)
        {
            return new Margin(one.Left + second.Left,
                              one.Top + second.Top,
                              one.Right + second.Right,
                              one.Bottom + second.Bottom);
        }
    }
}