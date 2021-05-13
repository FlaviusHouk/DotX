using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotX.Abstraction;
using DotX.Controls;

namespace DotX.Xaml.Generation
{
    public class ObjectComposer
    {
        public static void Compose(object objToCompose)
        {
            Type objType = objToCompose.GetType();
            string fullTypeName = objType.FullName;

            using Stream resource = 
                objType.Assembly.GetManifestResourceStream(string.Format("{0}.xaml", 
                                                                         fullTypeName));

            using var resourceReader = new System.IO.StreamReader(resource);
            using var xamlReader = new DotX.Xaml.XamlReader(resourceReader);

            XamlObject thisObj = xamlReader.Parse();
            var composer = new ObjectComposer(objToCompose, thisObj);
            composer.Compose();
        }

        private readonly object _target;
        private readonly XamlObject _description;

        public ObjectComposer(object target, 
                              XamlObject description)
        {
            _target = target;
            _description = description;

            Converters.Converters.RegisterConverter(typeof(IPropertyValue), new PropertyValueConverter());
        }

        public void Compose()
        {
            SetProperties(_target, _description.Properties);

            var childObjects = _description.Children.Select(c => ProcessObject(c)).ToArray();

            AssignContent(_target, childObjects);
        }

        private object ProcessObject(XamlObject obj)
        {
            object instance = Activator.CreateInstance(obj.ObjType);

            SetProperties(instance, obj.Properties);

            var childObjects = obj.Children.Select(c => ProcessObject(c)).ToArray();

            AssignContent(instance, childObjects);

            return instance;
        }

        private void SetProperties(object target,
                                   IEnumerable<XamlProperty> props)
        {
            foreach(var prop in props)
            {
                var inlineProp = prop as InlineXamlProperty;
                var fullProp = prop as FullXamlProperty;
                var extendedProp = prop as ExtendedXamlProperty;

                if(Converters.Converters.TryGetConverterForType(prop.PropertyType, out var converter) &&
                   inlineProp is not null)
                {
                    object val = converter.Convert(inlineProp.RawValue, prop.PropertyType);
                    PropertyInfo clrProp = target.GetType().GetProperty(prop.PropertyName);
                    
                    if(clrProp.CanWrite)
                    {
                        clrProp.SetValue(target, val);
                    }
                    else if(prop.PropertyType.GetInterfaces().Any(i => i.Name.StartsWith("ICollection")) &&
                            val is IEnumerable<object> enumerable)
                    {
                        var collection = clrProp.GetValue(target);
                        var methodToAdd = collection.GetType().GetMethod(nameof(ICollection<object>.Add));

                        foreach(var child in enumerable)
                            methodToAdd.Invoke(collection, new object[] { child });
                    }
                }
                else if(prop.PropertyType == typeof(string) &&
                        inlineProp is not null)
                {
                    target.GetType()
                          .GetProperty(prop.PropertyName)
                          .SetValue(target, inlineProp.RawValue);
                }
                else if (fullProp is not null)
                {
                    var clrProp = target.GetType()
                                        .GetProperty(prop.PropertyName);

                    var children = fullProp.Children.Select(c => ProcessObject(c))
                                                    .ToArray();
                    if(clrProp.CanWrite)
                    {
                        clrProp.SetValue(target, children);
                    }
                    else
                    {
                        var collection = clrProp.GetValue(target);
                        var methodToAdd = collection.GetType().GetMethod(nameof(ICollection<object>.Add));

                        foreach(var child in children)
                            methodToAdd.Invoke(collection, new object[] { child });
                    }  
                }
                else if(extendedProp is not null)
                {
                    var extension = (IMarkupExtension)ProcessObject(extendedProp.Extension);
                    var clrProp = target.GetType().GetProperty(extendedProp.PropertyName);

                    clrProp.SetValue(target, extension.ProvideValue(target, extendedProp.PropertyName));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void AssignContent(object target,
                                   IReadOnlyCollection<object> children)
        {
            if(!children.Any())
                return;
                
            if(target is Panel panel)
            {
                foreach(var child in children.Cast<Visual>())
                    panel.AddChild(child);
            }
            else if(target is Control control)
            {
                control.Content = children.Cast<Visual>().Single();
            }
            else
            {
                throw new Exception();
            }
        }
    }
}