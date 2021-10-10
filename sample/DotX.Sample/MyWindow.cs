using System;
using DotX.Widgets;
using DotX.Brush;
using DotX.Widgets.Extensions;

namespace DotX.Sample
{
    public partial class MyWindow : Window
    {
        private partial void LoadComponent();

        public MyWindow()
        {
            LoadComponent();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Random r = new();
            _InteractiveButton.Pressed+= args =>
            {
                Resources["ButtonBg"] = 
                    new SolidColorBrush(r.NextDouble(), 
                                        r.NextDouble(), 
                                        r.NextDouble());
            };
        }
    }
}