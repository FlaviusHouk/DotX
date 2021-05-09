namespace DotX.Abstraction
{
    public interface IPropertyValue 
    {
        T GetValue<T>();

        T SetValue<T>(T value);

        bool Is<T>();
    }
}