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
    public class FilesByHash : Component
    {

        public long Length
        {
            get
            {
                IContainer container = Container;
                if (container is FilesBySize.GroupedContainer)
                    return ((FilesBySize.GroupedContainer)container).Owner.Length;
                return -1L;
            }
        }

        public MD5Checksum? Checksum
        {
            get
            {
                ISite site = Site;
                if (null != site && site is IValueSite<MD5Checksum>)
                        return ((IValueSite<MD5Checksum>)site).Value;
                return null;
            }
        }

        public HashGroupContainer Group { get; }

        public FilesByHash()
        {
            Group = new HashGroupContainer(this);
        }
        
        public class HashGroupContainer : NestedContainer
        {
            
            public new FilesByHash Owner { get { return (FilesByHash)base.Owner; } }

            internal HashGroupContainer(FilesByHash owner) : base(owner)
            {
                if (null != owner.Group)
                    throw new InvalidOperationException();
            }

        }
        
    }
}