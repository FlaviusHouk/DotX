using System;
using System.Reflection;

namespace DotX.Xaml
{
    internal class XamlNamespace
    {
        public static XamlNamespace Parse(string key, string value)
        {
            if(key.Contains(':'))
                key = key.Split(':')[1];
            else
                key = string.Empty;

            return new XamlNamespace(key, value, "DotX");
        }

        public bool IsDefault => string.IsNullOrEmpty(Name);

        public string Name { get; } 

        public string ClrNamespace { get; }

        public string AssemblyName { get; }

        private XamlNamespace(string name, string clrNamespace, string assemblyName)
        {
            Name = name;
            ClrNamespace = clrNamespace;
            AssemblyName = assemblyName;
        }
    }
}