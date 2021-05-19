using DotX.Interfaces;
using Cairo;

namespace DotX.Brush
{
    public class SolidColorBrush : IBrush
    {
        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }

        public SolidColorBrush() {}
        public SolidColorBrush(double r,
                               double g,
                               double b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
        
        public void ApplyTo(Context context)
        {
            context.SetSourceRGB(Red, Green, Blue);
        }
    }
}