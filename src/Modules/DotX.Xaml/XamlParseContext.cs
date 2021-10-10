using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotX.Interfaces;
using DotX.Extensions;

namespace DotX.Xaml
{
    internal class XamlParseContext
    {
        public static XamlParseContext CreateRootContext(ILogger logger,
                                                         string[] referencedAssemblies)
        {
            return new (referencedAssemblies, logger);
        }

        private XamlParseContext _parent;

        private IList<XamlNamespace> _namespaces = 
            new List<XamlNamespace>();

        public ICollection<XamlNamespace> Namespaces => _namespaces;

        public XamlObject CurrentObject { get; set; }

        public XamlProperty CurrentProperty { get; set; }

        public XamlParseContext(XamlParseContext parentContext,
                                ILogger logger)
        {
            _parent = parentContext;
            _logger = logger;
        }

        private readonly string[] _references = 
            Array.Empty<string>();

        private readonly ILogger _logger;

        private XamlParseContext(string[] referencedAssemblies,
                                 ILogger logger)
        {
            _references = referencedAssemblies;
            _logger = logger;

            AppDomain.CurrentDomain.AssemblyResolve += LookupAssembly;
        }


        private Dictionary<string, Assembly> _assemblyCache = new();

        private Assembly LookupAssembly(object sender, ResolveEventArgs args)
        {
            string name = args.Name.Split(',').First().Trim();

            if(_assemblyCache.TryGetValue(name, out var reqAsm))
                return reqAsm;

            if(name == "DotX")
            {
                return AppDomain.CurrentDomain
                                .GetAssemblies()
                                .First(a => a.GetName().Name == name);
            }

            //_logger.LogWarning($"Looking for {args.Name}...");
            
            var path = _references.FirstOrDefault(r => Path.GetFileNameWithoutExtension(r) == name);
            //_logger.LogWarning($"Loading {path}...");

            if(string.IsNullOrEmpty(path))
                return default;

            reqAsm = Assembly.LoadFile(path);
            _assemblyCache.Add(name, reqAsm);
            return reqAsm;
        }

        public void AddNamespace(XamlNamespace ns)
        {
            _namespaces.Add(ns);
        }

        public Type LookupObjectByName(string objType, string ns)
        {
            if(ns == string.Empty)
            {
                foreach(var includedNs in _namespaces.Where(n => string.IsNullOrEmpty(n.Name)))
                {
                    string fullName = string.Format("{0}.{1}", 
                                                    includedNs.ClrNamespace, 
                                                    objType);

                    //Ugly way with assembly name...
                    if(TryLoad(fullName, includedNs.AssemblyName, out var t))
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

            if(t is null && 
               !string.IsNullOrEmpty(assemblyName)  && 
               _references.Any())
            {
                Assembly assembly = Assembly.Load(assemblyName);
                t = assembly.GetType(fullName, false, true); 
                //_logger.LogWarning($"Type {t?.FullName} loaded.");
            }

            return t is not null;
        }
    }
}