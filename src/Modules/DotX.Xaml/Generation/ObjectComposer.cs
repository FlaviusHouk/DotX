using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotX.Interfaces;
using DotX;
using DotX.Attributes;
using DotX.Data;

namespace DotX.Xaml.Generation
{
    public class ObjectComposer
    {
        static ObjectComposer()
        {
            Converters.Converters.RegisterConverter(typeof(IPropertyValue), new PropertyValueConverter());
        }
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
            var composer = new ObjectComposer();
            composer.Compose(objToCompose, thisObj);
        }

        private record ObjectInto (object Target, XamlObject Info)
        {}

        public void Compose(object target,
                            XamlObject desc)
        {
            SetProperties(target, desc.Properties);

            var childObjects = desc.Children.Select(c => ProcessObject(c)).ToArray();

            AssignContent(target, childObjects);
        }

        public object Build(XamlObject thisObj)
        {
            return ProcessObject(thisObj).Target;
        }

        private ObjectInto ProcessObject(XamlObject obj)
        {
            object instance = Activator.CreateInstance(obj.ObjType);

            SetProperties(instance, obj.Properties);

            var childObjects = obj.Children.Select(c => ProcessObject(c)).ToArray();

            AssignContent(instance, childObjects);

            return new ObjectInto(instance, obj);
        }

        private void SetProperties(object target,
                                   IEnumerable<XamlProperty> props)
        {
            foreach(var prop in props)
            {
                var inlineProp = prop as InlineXamlProperty;
                var fullProp = prop as FullXamlProperty;
                var extendedProp = prop as ExtendedXamlProperty;
                var attachedProp = prop as AttachedXamlProperty;

                if(attachedProp is not null)
                {
                    continue;
                }
                else if(Converters.Converters.TryGetConverterForType(prop.PropertyType, out var converter) &&
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
                else if(inlineProp is not null &&
                        prop.PropertyType.IsEnum &&
                        Enum.TryParse(prop.PropertyType, inlineProp.RawValue, true, out var enumValue))
                {
                    target.GetType()
                          .GetProperty(prop.PropertyName)
                          .SetValue(target, enumValue);
                }
                else if (fullProp is not null)
                {
                    var clrProp = target.GetType()
                                        .GetProperty(prop.PropertyName);

                    var children = fullProp.Children.Select(c => ProcessObject(c))
                                                    .ToArray();
                    if(clrProp.CanWrite)
                    {
                        clrProp.SetValue(target, children.Select(o => o.Target).ToArray());
                    }
                    else
                    {
                        var collection = clrProp.GetValue(target);
                        
                        if(collection is ResourceCollection res)
                        {
                            foreach(var child in children)
                            {
                                var keyAttr = child.Info.Properties.OfType<AttachedXamlProperty>().FirstOrDefault(p => p.Owner == "x");

                                res.Add(keyAttr.RawValue, child.Target);
                            }
                        }
                        else
                        {
                            var methodToAdd = collection.GetType().GetMethod(nameof(ICollection<object>.Add));

                            foreach(var child in children.Select(c => c.Target))
                                methodToAdd.Invoke(collection, new object[] { child });
                        }
                    }  
                }
                else if(extendedProp is not null)
                {
                    var extension = (IMarkupExtension)ProcessObject(extendedProp.Extension).Target;
                    var clrProp = target.GetType().GetProperty(extendedProp.PropertyName);

                    var extendedValue = extension.ProvideValue(target, extendedProp.PropertyName);

                    if(extendedValue is IPropertyValue propVal &&
                       target is CompositeObject compositeObject &&
                       compositeObject.TryGetProperty(extendedProp.PropertyName, out var compProp))
                    {
                        compositeObject.SetValue(compProp, propVal);
                    }
                    else
                    {
                        clrProp.SetValue(target, extendedValue);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void AssignContent(object target,
                                   IReadOnlyCollection<ObjectInto> children)
        {
            if(!children.Any())
                return;

            var targets = children.Select(c => c.Target);
            var contentAttr = target.GetType()
                                    .GetCustomAttribute<ContentMemberAttribute>(true);

            if (contentAttr.IsMethod)
            {
                MethodInfo addMethod = 
                    target.GetType()
                          .GetMethod(contentAttr.MemberName);

                foreach (var child in targets.Cast<Visual>())
                    addMethod.Invoke(target, new[] { child });
            }
            else
            {
                PropertyInfo prop = target.GetType()
                                          .GetProperty(contentAttr.MemberName);

                if (prop.PropertyType.IsAssignableFrom(typeof(Visual)))
                {
                    prop.SetValue(target, targets.Cast<Visual>().Single());
                }
                else if (prop.PropertyType == typeof(IVisualTreeGenerator) && children.Count == 1)
                {
                    prop.SetValue(target, new TemplateVisualGenerator(children.First().Info));
                }
                else
                {
                    throw new Exception();
                }
            }
        }
    }
}