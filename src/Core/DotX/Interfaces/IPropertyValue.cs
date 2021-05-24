using System;
using DotX.PropertySystem;

namespace DotX.Interfaces
{
    public interface IPropertyValue 
    {
        event Action<CompositeObject, CompositeObjectProperty> Attached;
        event Action<CompositeObject, CompositeObjectProperty> Changed;
        event Action<CompositeObject, CompositeObjectProperty> Detached;

        T GetValue<T>();

        T SetValue<T>(T value);

        bool Is<T>();

        void OnAttached(CompositeObject owner,
                        CompositeObjectProperty prop);

        void OnChanged(CompositeObject owner,
                       CompositeObjectProperty prop);

        void OnDetached(CompositeObject owner,
                        CompositeObjectProperty prop);
    }
}