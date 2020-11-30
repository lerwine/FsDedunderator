using System;
using System.ComponentModel;

namespace FsDedunderator
{
    public class FileNode
    {
        public SingleFile Grouped { get; }

        public FileStructureNode StructureNode { get; }

        public ISite Site { get ; set; }

        public string Name => StructureNode.Name;

        public long Length => Grouped.Length;

        public MD5Checksum? Checksum => Grouped.Checksum;

        public FileNode()
        {   
            Grouped = new SingleFile(this);
            StructureNode = new FileStructureNode(this);
        }
    }
}