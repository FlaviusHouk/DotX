using DotX.Interfaces;
using Cairo;

namespace DotX.Rendering
{
    public class InMemoryBackBufferFactory : IBackBufferFactory
    {
        public Surface CreateBuffer(int width, int height)
        {
            return new ImageSurface(Format.Argb32, 
                                    width,
                                    height);
        }
    }
}