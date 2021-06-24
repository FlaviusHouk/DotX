using DotX.PropertySystem;

namespace DotX.Widgets
{
    public static class TemplatedParentPropertyClass
    {
        public static readonly CompositeObjectProperty TemplatedParentProperty = 
            CompositeObjectProperty.RegisterProperty<Control, Visual>("TemplatedParent",
                                                                      PropertyOptions.None);
        
        public static Visual GetTemplatedParent(this Visual visual)
        {
            return visual.GetValue<Visual>(TemplatedParentProperty);
        }

        public static void SetTemplatedParent(this Visual visual, Control parent)
        {
            visual.SetValue(TemplatedParentProperty, parent);
        }
    }
}