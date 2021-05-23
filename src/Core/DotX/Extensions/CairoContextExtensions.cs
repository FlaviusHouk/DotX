using Cairo;

namespace DotX.Extensions
{
    public static class CairoContextExtensions
    {
        public static void Image(this Context context, ImageSource source)
        {
            Surface imageSurface;
            bool dispose = false;

            if(source is not CairoImageSource cairoImage)
            {
                byte[] bitmapData = source.GetBitmapData();
            
                imageSurface = 
                    new ImageSurface (bitmapData, 
                                      source.Format, 
                                      source.Width, 
                                      source.Height, 
                                      source.Stride);
                
                dispose = true;
            }
            else
            {
                imageSurface = cairoImage.ImageSource;
            }             

            context.SetSource(imageSurface);
            context.Fill();

            if(dispose)
                imageSurface.Dispose();
        }
    }
}