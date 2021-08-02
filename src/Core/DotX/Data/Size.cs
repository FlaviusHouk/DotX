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
    }
}