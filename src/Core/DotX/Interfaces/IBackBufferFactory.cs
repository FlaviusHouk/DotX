using Cairo;

namespace DotX.Interfaces
{
    public interface IBackBufferFactory
    {
        Surface CreateBuffer(int width, int height);
    }
}