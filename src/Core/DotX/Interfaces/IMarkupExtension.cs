namespace DotX.Interfaces
{
    public interface IMarkupExtension
    {
        object ProvideValue(object target, string propName);
    }
}