using System.IO;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Linq;
using DotX.Extensions;
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

        public ITaskItem[] References { get; set; }

        public override bool Execute()
        {
            var filesToAdd = new List<ITaskItem>();

            var projPath = Path.GetDirectoryName(CurrentProject.ItemSpec);
            var objPath = Path.Combine(projPath, "obj");

            Directory.CreateDirectory(objPath);
            string[] referencePaths = 
                References.Select(reference => reference.ItemSpec)
                          .ToArray();

            foreach(var filePath in InputFiles.Select(f => f.ItemSpec))
            {
                var name = Path.GetFileNameWithoutExtension(filePath);
                var fullName = Path.Combine(objPath, $"{name}.g.cs");

                /*if(!File.Exists(fullName))*/
                    filesToAdd.Add(new TaskItem(fullName));

                string ns = GetNamespace(filePath);

                using var xamlFile = File.OpenRead(filePath);
                using var xamlReader = new StreamReader(xamlFile);
                MsBuildLogger logger = new (Log);
                XamlReader r = new (xamlReader, referencePaths, logger);
                XamlObject obj = r.Parse();

                CodeGeneratorForObject gen = new (name, ns, logger);

                using var file = File.Open(fullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                file.SetLength(0);
                using StreamWriter writer = new (file);

                gen.GenerateCodeForObject(obj, writer);
		        writer.Flush();
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
