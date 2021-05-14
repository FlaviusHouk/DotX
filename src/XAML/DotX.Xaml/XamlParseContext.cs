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

        private IList<string> _rootNamespaces =
            new List<string>();

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

        public void IncludeIntoDefault(string ns)
        {
            if(string.IsNullOrEmpty(ns))
                throw new Exception();

            _rootNamespaces.Add(ns);
        }

        public Type LookupObjectByName(string objType, string ns)
        {
            if(ns == string.Empty)
            {
                foreach(var includedNs in _rootNamespaces)
                {
                    string fullName = string.Format("{0}.{1}", 
                                                    includedNs, 
                                                    objType);

                    //Ugly way with assembly name...
                    if(TryLoad(fullName, "DotX", out var t))
                        return t;
                }
            }
            else
            {
                var xNamespace = _namespaces.FirstOrDefault(n => n.Name == ns);
                
                if(xNamespace is not null)
                {
                    string fullName = string.Format("{0}.{1}", 
                                                    xNamespace.ClrNamespace, 
                                                    objType);

                    if(TryLoad(fullName, xNamespace.AssemblyName, out var t))
                        return t;
                }
            }

            return _parent?.LookupObjectByName(objType, ns);

        }

        private bool TryLoad(string fullName, string assemblyName, out Type t)
        {
            t = Type.GetType(fullName, false, true);

            if(t is null && !string.IsNullOrEmpty(assemblyName))
            {
                var assembly = Assembly.Load(assemblyName);

                t = assembly.GetType(fullName, false, true);
            }

            return t is not null;
        }
    }
}