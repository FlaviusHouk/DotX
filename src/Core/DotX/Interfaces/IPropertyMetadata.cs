namespace DotX.Interfaces
{
    public interface IPropertyMetadata
    {
        T Coerce<T>(CompositeObject obj, T value);

        void Changed<T>(CompositeObject obj, T oldVal, T newVal);
    }
}