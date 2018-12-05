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
    public sealed partial class ContentLengthGroup : ContentGroup<LengthOnlyFileReference>, IEquatable<ContentLengthGroup>
    {
        private long _length;

        /// <summary>
        /// 
        /// </summary>
        public override long Length => _length;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public ContentMD5Group.ChecksumsList Checksums { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name=""/></exception>
        public ContentLengthGroup(long length)
        {
            _length = length;
            Checksums = new ContentMD5Group.ChecksumsList(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ContentLengthGroup other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IFileReference> GetAllFileReferences() => FileReferences.Cast<IFileReference>().Concat(Checksums.SelectMany(c => c.GetAllFileReferences()));
    }
}