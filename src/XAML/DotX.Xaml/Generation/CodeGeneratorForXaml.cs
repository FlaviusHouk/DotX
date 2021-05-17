using System.IO;

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
            partial void LoadComponent() 
            {
                DotX.Xaml.Generation.ObjectComposer.Compose(this);
            }");

            output.WriteLine("\t}");

            output.WriteLine("}");
        }
    }
}