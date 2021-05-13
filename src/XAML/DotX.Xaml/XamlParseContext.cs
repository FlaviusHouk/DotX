using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotX.Xaml
{
    internal class XamlParseContext
    {
        public static XamlParseContext CreateRootContext()
        {
            return new XamlParseContext();
        }

        private XamlParseContext _parent;
        private IList<XamlNamespace> _namespaces = 
            new List<XamlNamespace>();

        public ICollection<XamlNamespace> Namespaces => _namespaces;

        public XamlObject CurrentObject { get; set; }

        public XamlProperty CurrentProperty { get; set; }

        public XamlParseContext(XamlParseContext parentContext)
        {
            _parent = parentContext;
        }

        private XamlParseContext()
        {}

        public void AddNamespace(XamlNamespace ns)
        {
            _namespaces.Add(ns);
        }

        public Type LookupObjectByName(string objType, string ns)
        {
            var xNamespace = _namespaces.FirstOrDefault(n => n.Name == ns);
            if(xNamespace is null)
                return _parent?.LookupObjectByName(objType, ns);

            var type = Type.GetType(string.Format("{0}.{1}", xNamespace.ClrNamespace, objType), false, true);

            if(type is null)
            {
                var assembly = Assembly.Load(xNamespace.AssemblyName);

                type = assembly.GetType(string.Format("{0}.{1}", xNamespace.ClrNamespace, objType), false, true);
            }

            return type;
        }
    }
}