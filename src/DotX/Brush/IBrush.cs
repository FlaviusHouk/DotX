using Cairo;

namespace DotX.Brush
{
    public interface IBrush
    {
        void Render(Context context, double width, double height);

        void ApplyTo(Context context);
    }
}