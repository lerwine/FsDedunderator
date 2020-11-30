using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FsDedunderator
{
    public class EnumeratedDirectory
    {
        public int MaxDepth { get; }
        public string RelativePath { get; }
        public string Name { get; }
        public ReadOnlyCollection<DirectoryInfo> Directories { get; } 
        public ReadOnlyCollection<FileInfo> Files { get; }
        private EnumeratedDirectory(DirectoryInfo directory, int maxDepth, string name, string relativePath)
        {
            MaxDepth = maxDepth;
            RelativePath = relativePath;
            Name = name;
            Directories = new ReadOnlyCollection<DirectoryInfo>((maxDepth > 0) ? directory.GetDirectories() : new DirectoryInfo[0]);
            Files = new ReadOnlyCollection<FileInfo>(directory.GetFiles());
        }
        
        internal EnumeratedDirectory(DirectoryInfo rootDirectory, int maxDepth)
            : this(rootDirectory, maxDepth, "", "") { }
        public IEnumerable<EnumeratedDirectory> EnumerateSubdirectories()
        {
            int m = MaxDepth - 1;
            if (RelativePath.Length == 0)
                return Directories.Select(d => new EnumeratedDirectory(d, m, d.Name, RelativePath));
            return Directories.Select(d => new EnumeratedDirectory(d, m, d.Name, Path.Combine(RelativePath, Name)));
        }
    }
}