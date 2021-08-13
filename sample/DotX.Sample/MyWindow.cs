using System;
using DotX.Widgets;
using DotX.Brush;
using DotX.Widgets.Extensions;

namespace DotX.Sample
{
    public partial class MyWindow : Window
    {
        partial void LoadComponent();

        public MyWindow()
        {
            LoadComponent();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if(this.TryFindChild<Button>(b => b.Name == "InteractiveButton", out Button but))
            {
                Random r = new();
                but.Pressed+= args =>
                {
                    Resources["ButtonBg"] = 
                        new SolidColorBrush(r.NextDouble(), 
                                            r.NextDouble(), 
                                            r.NextDouble());
                };
            }
        }
    }
}