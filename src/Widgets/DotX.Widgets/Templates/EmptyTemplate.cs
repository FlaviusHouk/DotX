using DotX.Interfaces;

namespace DotX.Widgets.Templates
{
    public class EmptyTemplate : Template
    {
        public override void ApplyTo(Control c)
        {
            c.Child = c.Content;
            c.Content.VisualParent = c;

            if(c.Child is IInitializable initializable)
                initializable.Initialize();

            if(c.Content is Widget w)
                w.ApplyStyles();
        }
    }
}