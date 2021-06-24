using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace DotX.Xaml
{
    public class XamlReader : IDisposable
    {
        public static XamlObject ParseFile(string fileName)
        {
            using var file = File.Open(fileName, FileMode.Open, FileAccess.Read);
            using var fileReader = new StreamReader(file);

            using var xamlReader = new XamlReader(fileReader);

            return xamlReader.Parse(); 
        }

        private readonly XmlReader _reader;
        private readonly Stack<XamlParseContext> _contexts = 
            new Stack<XamlParseContext>();
        
        private XamlParseContext CurrentContext => _contexts.Peek();

        public XamlReader(TextReader input)
        {
            _reader = new XmlTextReader(input);
            _contexts.Push(XamlParseContext.CreateRootContext());

            //TODO: Add option to modify it by user
            CurrentContext.AddNamespace(new XamlNamespace(string.Empty, "DotX.Styling", "DotX"));
            CurrentContext.AddNamespace(new XamlNamespace(string.Empty, "DotX.Xaml.MarkupExtensions", "DotX.Xaml"));
            CurrentContext.AddNamespace(new XamlNamespace(string.Empty, "DotX.Brush", "DotX"));
        }

        public XamlObject Parse()
        {
            XamlObject root = default;

            while(_reader.Read())
            {
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:
                        XamlObject readObj = ReadObject();

                        root ??= readObj;

                        break;
                    
                    case XmlNodeType.EndElement:
                        if(CurrentContext.CurrentProperty is not null)
                        {
                            CurrentContext.CurrentObject.AddProperty(CurrentContext.CurrentProperty);
                            CurrentContext.CurrentProperty = default;
                            break;
                        }

                        ExitScope();
                        break;
                }
            }

            return root;
        }

        private XamlObject ReadObject()
        {
            XamlParseContext parentContext = CurrentContext;
            bool createNewScope = !_reader.IsEmptyElement;
            string elementName = _reader.Name;
            string ns = string.Empty;

            if(elementName.Contains(':'))
            {
                var parts = elementName.Split(':', StringSplitOptions.RemoveEmptyEntries);

                ns = parts[0];
                elementName = parts[1];
            }

            if(elementName.Contains('.'))
            {
                string[] parts = elementName.Split('.', StringSplitOptions.RemoveEmptyEntries);

                if(parts.Length > 2)
                    throw new Exception();

                CurrentContext.CurrentProperty = new FullXamlProperty(parts[1]);
                return null;
            }

            if(createNewScope)
                EnterScope();

            var attributes = new Dictionary<string, string>();

            if(_reader.HasAttributes)
            {
                _reader.MoveToFirstAttribute();

                do
                {
                    attributes.Add(_reader.Name, _reader.Value);
                }
                while(_reader.MoveToNextAttribute());

                AddNamespaces(attributes);                
            }

            Type objType = CurrentContext.LookupObjectByName(elementName, ns);

            var obj = new XamlObject(objType);            

            foreach(var attr in attributes)
            {
                XamlProperty prop;
                string trimmedValue = attr.Value.Trim();

                if(trimmedValue.StartsWith('{') && 
                   trimmedValue.EndsWith('}'))
                {
                    var parser = new MarkupExtensionParser(trimmedValue);
                    var extension = parser.ParseExtension(CurrentContext);
                    prop = new ExtendedXamlProperty(attr.Key, extension);
                }
                else if(attr.Key.Contains(':'))
                {
                    var parts = attr.Key.Split(':');

                    if(parts.Length != 2)
                        throw new XmlException();

                    prop = new AttachedXamlProperty(parts[1], attr.Value, parts[0]);
                }
                else
                {
                    prop = new InlineXamlProperty(attr.Key, attr.Value);
                }

                obj.AddProperty(prop);
            }
            
            if(createNewScope)
                CurrentContext.CurrentObject = obj;

            if(parentContext.CurrentProperty is not null &&
               parentContext.CurrentProperty is FullXamlProperty p)
            {
                p.AddChild(obj);
            }
            else
            {
                parentContext.CurrentObject?.AddToContent(obj);
            }

            return obj;
        }

        private void EnterScope()
        {
            var newContext = new XamlParseContext(CurrentContext);
            _contexts.Push(newContext);
        }

        private void ExitScope()
        {
            _contexts.Pop();
        }
        
        private void AddNamespaces(Dictionary<string, string> attributes)
        {
            var nsAttr = attributes.Where(a => a.Key.StartsWith("xmlns")).ToArray();
            XamlParseContext context = CurrentContext;

            foreach(var ns in nsAttr)
            {
                attributes.Remove(ns.Key);

                if(XamlNamespace.TryParse(ns.Key, ns.Value, out var xamlNs))
                    context.AddNamespace(xamlNs);
                else
                    throw new Exception();
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
