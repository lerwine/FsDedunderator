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
    public class SingleFile : Component
    {
        public FileNode Owner { get; }

        public string Name => Owner.Name;
        
        public long Length
        {
            get
            {
                ISite site = Site;
                if (null != site && site is IValueSite<long>)
                {
                    if (site is IValueSite<long>)
                        return ((IValueSite<long>)site).Value;
                    if (site.Container is FilesBySize.GroupedContainer)
                        return ((FilesBySize.GroupedContainer)site.Container).Owner.Length;
                    if (site.Container is FilesByHash.HashGroupContainer)
                        return ((FilesByHash.HashGroupContainer)site.Container).Owner.Length;
                }
                return -1L;
            }
        }

        public MD5Checksum? Checksum
        {
            get
            {
                ISite site = Site;
                if (null != site && site is IValueSite<long>)
                {
                    if (site is IValueSite<MD5Checksum>)
                        return ((IValueSite<MD5Checksum>)site).Value;
                    if (site.Container is FilesByHash.HashGroupContainer)
                        return ((FilesByHash.HashGroupContainer)site.Container).Owner.Checksum;
                }
                return null;
            }
        }

        internal SingleFile(FileNode owner)
        {
            if (null == owner)
                throw new ArgumentNullException("owner");
            if (null != owner.Grouped)
                throw new InvalidOperationException();
            Owner = owner;
        }
    }
}