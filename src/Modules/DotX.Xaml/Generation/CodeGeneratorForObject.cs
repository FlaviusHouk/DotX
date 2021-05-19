using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotX.Interfaces;
using DotX;

namespace DotX.Xaml.Generation
{
    /*public class CodeGeneratorForObject
    {
        private readonly string _className;
        private readonly string _ns;

        private readonly Dictionary<Type, int> _nameCounter =
            new Dictionary<Type, int>();

        public CodeGeneratorForObject(string className, string ns)
        {
            _className = className;
            _ns = ns;
        }

        public void GenerateCodeForObject(XamlObject obj, TextWriter output)
        {
            output.WriteLine($"namespace {_ns};");
            output.WriteLine("{");
            output.WriteLine($"partial class {_className}");
            output.WriteLine("{");
            output.WriteLine("private void Initialize()\n{");
            
            var converters = AppDomain.CurrentDomain.GetAssemblies()
                                                    .SelectMany(ass => ass.GetTypes()
                                                                          .Where(t => t.GetInterface(nameof(IValueConverter)) is not null))
                                                    .ToDictionary(t => t.GetCustomAttribute<ConverterForTypeAttribute>().TargetType,
                                                                  t => (IValueConverter)Activator.CreateInstance(t));

            GenerateProperties(output, obj.Properties, "this", converters);

            var childObjects = obj.Children.Select(child => GenerateCodeForObject(child, output, converters)).ToArray();

            output.WriteLine();

            AssignContent(obj, output, "this", childObjects);
        }

        private string GenerateCodeForObject(XamlObject obj, 
                                             TextWriter output, 
                                             IDictionary<Type, IValueConverter> converters)
        {
            string objName = GetObjectName(obj.ObjType);

            output.WriteLine($"{obj.ObjType.Name} {objName} = new ();");

            GenerateProperties(output, obj.Properties, objName, converters);

            var childObjects = obj.Children.Select(child => GenerateCodeForObject(child, output, converters)).ToArray();

            AssignContent(obj, output, objName, childObjects);

            output.WriteLine();

            return objName;
        }

        private void GenerateProperties(TextWriter output,
                                        IEnumerable<XamlProperty> props,
                                        string objName,
                                        IDictionary<Type, IValueConverter> converters)
        {
            foreach(var prop in props)
            {
                output.Write($"{objName}.{prop.PropertyName} = ");
                var converter = converters[prop.PropertyType];

                if(prop.PropertyType.IsPrimitive && prop is InlineXamlProperty ixp)
                {
                    object val = converter.Convert(ixp.RawValue, prop.PropertyType);

                    output.Write($"{val.ToString()};");
                }
            }
        }

        private void AssignContent(XamlObject obj,
                                   TextWriter output,
                                   string objName,
                                   IReadOnlyCollection<string> children)
        {
            if(obj.ObjType.IsSubclassOf(typeof(Panel)))
            {
                foreach(var child in children)
                    output.Write($"{objName}.{nameof(Panel.AddChild)}({child});");
            }
            else if(obj.ObjType.IsSubclassOf(typeof(Control)))
            {
                output.Write($"{objName}.{nameof(Control.Content)} = {children.Single()};");
            }
            else
            {
                throw new Exception();
            }
        }

        private string GetObjectName(Type objType)
        {
            if(!_nameCounter.TryGetValue(objType, out int counter))
            {
                _nameCounter.Add(objType, 1);
                
                return $"{objType.Name}_1";
            }

            counter++;
            var name = $"{objType.Name}_{counter}";
            _nameCounter[objType] = counter;

            return name;
        }
    }*/
}