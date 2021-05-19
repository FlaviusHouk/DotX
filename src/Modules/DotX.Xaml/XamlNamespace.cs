using System;
using System.Reflection;

namespace DotX.Xaml
{
    internal class XamlNamespace
    {
        public static bool TryParse(string key, string value, out XamlNamespace ns)
        {
            ns = default;
            var assembly = "DotX";
            string clrNamespace = value;

            if(key.Contains(':'))
                key = key.Split(':')[1];
            else
                key = string.Empty;

            if(value.Contains(';'))
            {
                foreach(var par in value.Split(';'))
                {
                    var parts = par.Split(':', StringSplitOptions.RemoveEmptyEntries);

                    switch(parts[0])
                    {
                        case "clr-namespace":
                            clrNamespace = parts[1];
                            break;
                        case "assembly":
                            assembly = parts[1];
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }
            
            ns = new XamlNamespace(key, clrNamespace, assembly);
            
            return true;
        }

        public string Name { get; } 

        public string ClrNamespace { get; }

        public string AssemblyName { get; }

        internal XamlNamespace(string name, string clrNamespace, string assemblyName)
        {       
            Name = name;
            ClrNamespace = clrNamespace;
            AssemblyName = assemblyName;
        }
    }
}