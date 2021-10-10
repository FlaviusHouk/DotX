using DotX.Attributes;
using DotX.Interfaces;

namespace DotX.Widgets.Templates
{
    [ContentMember(nameof(Template.ContentGenerator))]
    public class Template
    {
        public IVisualTreeGenerator ContentGenerator { get; set; }

        public virtual void ApplyTo(Control c)
        {
            c.Child = ContentGenerator.GenerateRootVisual();

            if(c.Child is Widget childWidget)
                SetTemplatedParentRecursive(c, childWidget);
            else
                c.Child.SetTemplatedParent(c);

            if(c.Child is IInitializable initializable)
                initializable.Initialize();
        }

        private void SetTemplatedParentRecursive(Control parent, Widget w)
        {
            foreach(var child in w.VisualChildren)
            {
                child.SetTemplatedParent(parent);

                if(child is Widget childWidget)
                    SetTemplatedParentRecursive(parent, childWidget);
            }
        }
    }
}