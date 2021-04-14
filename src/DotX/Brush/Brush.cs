using Cairo;

namespace DotX.Brush
{
    public interface IBrush
    {
        void Render(Context context, int width, int height);
    }
}