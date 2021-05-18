using System;
using System.Reflection;

namespace DotX.Xaml
{
    internal class XamlNamespace
    {
        public static bool TryParse(string key, string value, out XamlNamespace ns)
        {
            ns = default;

            if(key.Contains(':'))
                key = key.Split(':')[1];
            else
                return false;

            ns = new XamlNamespace(key, value, "DotX");
            
            return true;
        }

        public string Name { get; } 

        public string ClrNamespace { get; }

        public string AssemblyName { get; }

        private XamlNamespace(string name, string clrNamespace, string assemblyName)
        {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
                
            Name = name;
            ClrNamespace = clrNamespace;
            AssemblyName = assemblyName;
        }
    }
}