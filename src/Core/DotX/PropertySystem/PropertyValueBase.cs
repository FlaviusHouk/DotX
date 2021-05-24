using System;
using DotX.Interfaces;

namespace DotX.PropertySystem
{
    public abstract class PropertyValueBase : IPropertyValue
    {
        public event Action<CompositeObject, CompositeObjectProperty> Attached;
        public event Action<CompositeObject, CompositeObjectProperty> Changed;
        public event Action<CompositeObject, CompositeObjectProperty> Detached;

        public abstract T GetValue<T>();

        public abstract bool Is<T>();

        public abstract T SetValue<T>(T value);

        public void OnAttached(CompositeObject owner, CompositeObjectProperty prop)
        {
            Attached?.Invoke(owner, prop);
        }

        public void OnChanged(CompositeObject owner, CompositeObjectProperty prop)
        {
            Changed?.Invoke(owner, prop);
        }

        public void OnDetached(CompositeObject owner, CompositeObjectProperty prop)
        {
            Detached?.Invoke(owner, prop);
        }
    }
}