using DotX.Widgets;

namespace DotX.Sample
{
    public partial class MyWindow : Window
    {
        partial void LoadComponent();

        public MyWindow()
        {
            LoadComponent();
        }
    }
}