using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FsDedunderator
{
    public class FileStructureNode : Component
    {
        public string Name
        {
            get
            {
                ISite site = Site;
                if (null != site && site is IValueSite<string>)
                    return ((IValueSite<string>)site).Value;
                return null;
            }
        }

        public long Length => Owner.Length;

        public MD5Checksum? Checksum => Owner.Checksum;

        public FileNode Owner { get; }

        internal FileStructureNode(FileNode owner)
        {
            if (null == owner)
                throw new ArgumentNullException("owner");
            if (null != owner.StructureNode)
                throw new InvalidOperationException();
            Owner = owner;
        }
    }
}