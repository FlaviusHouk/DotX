using System.IO;
using DotX.Controls;
using DotX.Xaml;
using DotX.Xaml.Generation;

namespace DotX.Sample
{
    public partial class MyWindow : Window
    {
        partial void Initialize();

        public MyWindow()
        {
            Initialize();
        }
    }
}