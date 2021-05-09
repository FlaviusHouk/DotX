namespace DotX.Abstraction
{
    public interface IPropertyMetadata
    {
        T Coerce<T>(CompositeObject obj, T value);

        IChangeHandler InitiateChange(CompositeObject obj,
                                      CompositeObjectProperty prop);

        void Changed<T>(CompositeObject obj, T oldVal, T newVal);
    }
}