namespace DotX.Abstraction
{
    public interface IMarkupExtension
    {
        object ProvideValue(object target, string propName);
    }
}