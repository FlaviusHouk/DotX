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
            
            if(createNewScope)
                EnterScope();

            var attributes = new Dictionary<string, string>();

            string elementName = _reader.Name;

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
            
            Type objType = CurrentContext.LookupObjectByName(elementName);
            var obj = new XamlObject(objType);            

            foreach(var attr in attributes)
            {
                var prop = new XamlProperty(attr.Key, attr.Value);
                obj.AddProperty(prop);
            }
            
            if(createNewScope)
                CurrentContext.CurrentObject = obj;

            parentContext.CurrentObject?.AddToContent(obj);

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

                context.AddNamespace(XamlNamespace.Parse(ns.Key, ns.Value));
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
