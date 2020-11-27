using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public partial class ContentLengthGroup
    {
        public sealed partial class ContentMD5Group : ContentGroup<MD5CalculatedFileReference>, IEquatable<ContentMD5Group>
        {
            private ChecksumsList _owner;
            public override long Length
            {
                get
                {
                    ChecksumsList owner = _owner;
                    if (owner == null)
                        throw new InvalidOperationException("Item has been removed from the owning collection");
                    return owner.Content.Length;
                }
            }

            internal ChecksumsList Owner => _owner;
            public MD5Checksum Checksum { get; }
            protected override MD5Checksum? BaseChecksum => Checksum;
            internal ContentMD5Group(ContentLengthGroup parent, MD5Checksum checksum)
            {
                _owner = parent.Checksums;
                Checksum = checksum;
            }
            public override IEnumerable<IFileReference> GetAllFileReferences() => FileReferences.Cast<IFileReference>();
            public bool Equals(ContentMD5Group other)
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
        }
    }
}