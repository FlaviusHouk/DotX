using System;
using System.Collections.Generic;
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

        public Type LookupObjectByName(string objType)
        {
            foreach(var ns in _namespaces)
            {
                var type = Type.GetType(string.Format("{0}.{1}", ns.ClrNamespace, objType), false, true);

                if(type is null)
                {
                    var assembly = Assembly.Load(ns.AssemblyName);
                    
                    type = assembly.GetType(string.Format("{0}.{1}", ns.ClrNamespace, objType), false, true);
                }

                if(type is not null)
                    return type;
            }

            return _parent?.LookupObjectByName(objType);
        }
    }
}