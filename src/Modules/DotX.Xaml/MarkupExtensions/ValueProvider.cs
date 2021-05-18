using DotX.Interfaces;

namespace DotX.Xaml.MarkupExtensions
{
    //Dummy class just to check that providers work.
    public class ValueProvider : IMarkupExtension
    {
        public string RawValue { get; set; }

        public object ProvideValue(object target, string propName)
        {
            var prop = target.GetType().GetProperty(propName);

            var converter = Converters.Converters.GetConverterForType(prop.PropertyType);

            return converter.Convert(RawValue, prop.PropertyType);
        }
    }
}