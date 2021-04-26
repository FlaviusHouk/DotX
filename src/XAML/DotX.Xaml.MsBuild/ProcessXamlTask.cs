using System.IO;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Linq;

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

                if(!File.Exists(fullName))
                    filesToAdd.Add(new TaskItem(fullName));

                File.WriteAllText(fullName, @"using System; namespace DotX { class A { public static void Meth() {}}}");
            }

            FilesToAdd = filesToAdd.ToArray();
            return true;
        }
    }
}
