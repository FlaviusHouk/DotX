using Cairo;

namespace DotX
{
    public abstract class ImageSource
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract Format Format { get; }
        public abstract int Stride { get; }

        public abstract byte[] GetBitmapData();

        public abstract void CopyPixels(byte[] dest, int startIndex, int count);
    }
}