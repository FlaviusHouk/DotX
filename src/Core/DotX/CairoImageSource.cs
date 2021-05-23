using System;
using System.IO;
using Cairo;

namespace DotX
{
    internal class CairoImageSource : ImageSource
    {
        private ImageSurface _imageSource;
        public Surface ImageSource => _imageSource;

        public override int Width => _imageSource.Width;

        public override int Height => _imageSource.Height;

        public override Format Format => _imageSource.Format;

        public override int Stride => _imageSource.Stride;

        public CairoImageSource(string file)
        {
            var fileExtension = System.IO.Path.GetExtension(file);

            if(fileExtension != ".png")
                throw new Exception($"Cannot open {fileExtension} file.");

            _imageSource = new ImageSurface(file);

            if(_imageSource.Status != Status.Success)
                throw new Exception($"Cannot load image. Having {_imageSource.Status}.");
        }

        public override void CopyPixels(byte[] dest, int startIndex, int count)
        {
            _imageSource.Data.CopyTo(dest, count);
        }

        public override byte[] GetBitmapData()
        {
            return _imageSource.Data;
        }
    }
}