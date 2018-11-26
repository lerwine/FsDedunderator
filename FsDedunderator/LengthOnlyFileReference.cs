using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FsDedunderator
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LengthOnlyFileReference : FileReference<ContentLengthGroup>, IEquatable<LengthOnlyFileReference>
    {
        internal LengthOnlyFileReference(DirectoryFile file, ContentLengthGroup content) : base(file, content) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(LengthOnlyFileReference other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(IFileReference other)
        {
            throw new NotImplementedException();
        }
    }
}