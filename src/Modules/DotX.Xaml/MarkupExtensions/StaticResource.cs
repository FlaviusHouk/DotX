using System;
using DotX.Interfaces;
using DotX;

namespace DotX.Xaml.MarkupExtensions
{
    public class StaticResource : IMarkupExtension
    {
        public string Key { get; set; }

        public object ProvideValue(object target, string propName)
        {
            if(target is not Visual current)
                throw new Exception();

            if(target is IResourceOwner w)
            {
                return new ResourcePropertyValue(Key, w);    
            }

            throw new InvalidOperationException();
        }
    }
}