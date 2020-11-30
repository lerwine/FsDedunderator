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
    public class FilesBySize : Component
    {
        private GroupedContainer Group { get; }

        public long Length
        {
            get
            {
                ISite site = Site;
                if (null != site && site is IValueSite<long> && site is IValueSite<long>)
                        return ((IValueSite<long>)site).Value;
                return -1L;
            }
        }

        public FilesBySize()
        {
            Group = new GroupedContainer(this);
        }
        
        internal void AddFile(SingleFile singleFile)
        {
            throw new NotImplementedException();
        }

        public sealed class GroupedContainer : NestedValueContainer<MD5Checksum>
        {
            public new FilesBySize Owner { get { return (FilesBySize)base.Owner; } }

            internal GroupedContainer(FilesBySize owner) : base(owner)
            {
                if (null != owner.Group)
                    throw new InvalidOperationException();
            }

            protected override string ValidateName(IComponent component, string name, out MD5Checksum value)
            {
                throw new NotImplementedException();
            }

            protected override MD5Checksum ValidateValue(IComponent component, MD5Checksum value, out string name)
            {
                throw new NotImplementedException();
            }

            protected override bool AreValuesEqual(MD5Checksum x, MD5Checksum y)
            {
                throw new NotImplementedException();
            }

            protected override Site CreateSite(IComponent component, string name)
            {
                throw new NotImplementedException();
            }

            protected override ValueSite CreateValueSite(IComponent component, MD5Checksum value)
            {
                throw new NotImplementedException();
            }

            private class SingleFileSite : ValueSite
            {
                public new SingleFile Component { get { return (SingleFile)base.Component; } }

                public SingleFileSite(SingleFile component, NestedValueContainer<MD5Checksum> container, MD5Checksum value) : base(component, container, value)
                {
                }
            }

            private class GroupedFileSite : ValueSite
            {
                public new FilesByHash Component { get { return (FilesByHash)base.Component; } }

                internal GroupedFileSite(FilesByHash component, GroupedContainer container, MD5Checksum value) : base(component, container, value) { }
            }
        }
    }
}