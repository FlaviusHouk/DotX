using DotX.Interfaces;

namespace DotX.Widgets.Templates
{
    public class Template
    {
        public IVisualTreeGenerator ContentGenerator { get; set; }

        public virtual void ApplyTo(Control c)
        {
            c.Child = ContentGenerator.GenerateRootVisual();

            if(c.Child is IInitializable initializable)
                initializable.Initialize();
        }
    }
}