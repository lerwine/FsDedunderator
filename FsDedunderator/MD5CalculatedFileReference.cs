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
    public sealed class MD5CalculatedFileReference : FileReference<ContentLengthGroup.ContentMD5Group>, IEquatable<MD5CalculatedFileReference>
    {
        /// <summary>
        /// 
        /// </summary>
        public long LastCalculated { get; }

        internal MD5CalculatedFileReference(DirectoryFile file, DateTime lastCalculated, ContentLengthGroup.ContentMD5Group content) : base(file, content)
        {
            LastCalculated = lastCalculated.ToFileTimeUtc();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MD5CalculatedFileReference other)
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