using System.IO;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Linq;
using DotX.Xaml.Generation;

namespace DotX.Xaml.MsBuild
{
    public class ProcessXamlTask : Task
    {
        [Required]
        public ITaskItem[] InputFiles { get; set; }

        [Output]
        public ITaskItem[] FilesToAdd { get; set; }

        [Required]
        public ITaskItem CurrentProject { get; set; }

        public override bool Execute()
        {
            var filesToAdd = new List<ITaskItem>();

            var projPath = Path.GetDirectoryName(CurrentProject.ItemSpec);
            var objPath = Path.Combine(projPath, "obj");

            Directory.CreateDirectory(objPath);

            foreach(var filePath in InputFiles.Select(f => f.ItemSpec))
            {
                var name = Path.GetFileNameWithoutExtension(filePath);
                var fullName = Path.Combine(objPath, $"{name}.g.cs");

                /*if(!File.Exists(fullName))*/
                    filesToAdd.Add(new TaskItem(fullName));

                string ns = GetNamespace(filePath);
                var generator = new CodeGeneratorForXaml(name, ns);

                using var file = File.Open(fullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                file.SetLength(0);
                using StreamWriter writer = new (file);

                generator.Generate(writer);


                /*XamlReader r = new (filePath);
                XamlObject obj = r.Parse();

                CodeGenerator gen = new (name, "DotX.Sample");

                using var file = File.Open(fullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                file.SetLength(0);
                using StreamWriter writer = new (file);

                gen.GenerateCodeForObject(obj, writer);*/
            }

            FilesToAdd = filesToAdd.ToArray();

            return true;
        }

        private string GetNamespace(string filePath)
        {
            string baseName = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            string csFile = Path.Combine(baseName, $"{fileName}.cs");

            if(!File.Exists(csFile))
                return string.Empty;

            using var file = File.OpenRead(csFile);
            using var textReader = new StreamReader(file);

            string line = textReader.ReadLine().Trim();
            const string nsKeyword = "namespace"; 
            while(!line.StartsWith(nsKeyword))
            {
                line = textReader.ReadLine().Trim();
            }

            return line.Substring(nsKeyword.Length).Trim(new char[] { ' ', '\t', '{', '\r', '\n' });
        }
    }
}
