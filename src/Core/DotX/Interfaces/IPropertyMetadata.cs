using DotX.PropertySystem;

namespace DotX.Interfaces
{
    public interface IPropertyMetadata
    {
        PropertyOptions Options { get; }
        
        T Coerce<T>(CompositeObject obj, T value);

        void Changed<T>(CompositeObject obj, T oldVal, T newVal);
    }
}