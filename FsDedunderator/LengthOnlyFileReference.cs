using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    public sealed class LengthOnlyFileReference : FileReference<ContentLengthGroup>, IEquatable<LengthOnlyFileReference>
    {
        internal LengthOnlyFileReference(DirectoryFile file, ContentLengthGroup content) : base(file, content) { }

        public bool Equals(LengthOnlyFileReference other)
        {
            throw new NotImplementedException();
        }
        public override bool Equals(IFileReference other)
        {
            throw new NotImplementedException();
        }
    }
}