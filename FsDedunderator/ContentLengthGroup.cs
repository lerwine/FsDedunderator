using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public sealed partial class ContentLengthGroup : ContentGroup<LengthOnlyFileReference>, IEquatable<ContentLengthGroup>
    {
        private long _length;

        public override long Length => _length;

        public ContentMD5Group.ChecksumsList Checksums { get; }

        public ContentLengthGroup(long length)
        {
            _length = length;
            Checksums = new ContentMD5Group.ChecksumsList(this);
        }

        public bool Equals(ContentLengthGroup other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            throw new NotImplementedException();
        }
        public override IEnumerable<IFileReference> GetAllFileReferences() => FileReferences.Cast<IFileReference>().Concat(Checksums.SelectMany(c => c.GetAllFileReferences()));
    }
}