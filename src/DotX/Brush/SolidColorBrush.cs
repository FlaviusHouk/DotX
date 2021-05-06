using Cairo;

namespace DotX.Brush
{
    public class SolidColorBrush : IBrush
    {
        private readonly double _r;
        private readonly double _g;
        private readonly double _b;

        public SolidColorBrush(double r,
                               double g,
                               double b)
        {
            _r = r;
            _g = g;
            _b = b;
        }
        
        public void ApplyTo(Context context)
        {
            context.SetSourceRGB(_r, _g, _b);
        }
    }
}