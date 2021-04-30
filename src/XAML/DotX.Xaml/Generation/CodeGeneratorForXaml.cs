using System.IO;
using DotX.Abstraction;

namespace DotX.Xaml.Generation
{
    public class CodeGeneratorForXaml
    {
        private readonly string _fileName;
        private readonly string _ns;

        public CodeGeneratorForXaml(string fileName, string ns)
        {
            _fileName = fileName;
            _ns = ns;
        }

        public void Generate(TextWriter output)
        {
            output.WriteLine($"namespace {_ns}");
            output.WriteLine("{");

            output.WriteLine($"\tpublic partial class {_fileName}");
            output.WriteLine("\t{");

            output.WriteLine(@"
            
            partial void Initialize() 
            {
                var fullTypeName = GetType().FullName;

                using var resource = 
                    GetType().Assembly.GetManifestResourceStream(string.Format(""{0}.xaml"", 
                                                                               fullTypeName));

                using var resourceReader = new System.IO.StreamReader(resource);
                using var xamlReader = new DotX.Xaml.XamlReader(resourceReader);

                var thisObj = xamlReader.Parse();
                var composer = new DotX.Xaml.Generation.ObjectComposer(this, thisObj);
                composer.Compose();
            }");

            output.WriteLine("\t}");

            output.WriteLine("}");
        }
    }
}