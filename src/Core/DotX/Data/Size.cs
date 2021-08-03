namespace DotX.Data
{
    public readonly struct Size
    {
        public double Width { get; init; }
        public double Height { get; init; }

        public Size(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public void Deconstruct(out double width, out double height)
        {
            width = Width;
            height = Height;
        }

        public static bool operator ==(Size one, Size second)
        {
            return one.Width == second.Width &&
                   one.Height == second.Height;
        }

        public static bool operator != (Size one, Size second)
        {
            return one.Width != second.Width ||
                   one.Height != second.Height;
        }
    }
}