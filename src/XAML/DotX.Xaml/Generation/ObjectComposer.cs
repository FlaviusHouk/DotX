using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotX.Abstraction;
using DotX.Controls;

namespace DotX.Xaml.Generation
{
    public class ObjectComposer
    {
        private readonly object _target;
        private readonly XamlObject _description;

        private readonly Dictionary<Type, IValueConverter> _converters;

        public ObjectComposer(object target, 
                              XamlObject description)
        {
            _target = target;
            _description = description;

            _converters = AppDomain.CurrentDomain.GetAssemblies()
                                                 .SelectMany(ass => ass.GetTypes()
                                                                       .Where(t => t.GetInterface(nameof(IValueConverter)) is not null))
                                                 .ToDictionary(t => t.GetCustomAttribute<ConverterForTypeAttribute>().TargetType,
                                                               t => (IValueConverter)Activator.CreateInstance(t));
        }

        public void Compose()
        {
            SetProperties(_target, _description.Properties, _converters);

            var childObjects = _description.Children.Select(c => ProcessObject(c, _converters)).ToArray();

            AssignContent(_target, childObjects);
        }

        private object ProcessObject(XamlObject obj,
                                     IDictionary<Type, IValueConverter> converters)
        {
            object instance = Activator.CreateInstance(obj.ObjType);

            SetProperties(instance, obj.Properties, converters);

            var childObjects = obj.Children.Select(c => ProcessObject(c, converters)).ToArray();

            AssignContent(instance, childObjects);

            return instance;
        }

        private void SetProperties(object target,
                                   IEnumerable<XamlProperty> props,
                                   IDictionary<Type, IValueConverter> converters)
        {
            foreach(var prop in props)
            {
                if(converters.TryGetValue(prop.PropertyType, out var converter))
                {
                    object val = converter.Convert(prop.RawValue, prop.PropertyType);

                    target.GetType()
                          .GetProperty(prop.PropertyName)
                          .SetValue(target, val);
                }
                else if(prop.PropertyType == typeof(string))
                {
                    target.GetType()
                          .GetProperty(prop.PropertyName)
                          .SetValue(target, prop.RawValue);
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