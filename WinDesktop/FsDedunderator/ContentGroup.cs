using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FsDedunderator
{
    /// <summary>
    /// Base class for objects that group files by the characteristics of their content.
    /// </summary>
    /// <typeparam name="T">Type of object being grouped.</typeparam>
    public abstract partial class ContentGroup<T> :  IContentGroup
        where T : IFileReference
    {
        [NonSerialized]
        private FileReferenceListWrapper _innerList = null;

        /// <summary>
        /// Length of all files in the current <see cref="ContentGroup{T}" /> in bytes.
        /// </summary>
        public abstract long Length { get; }

        /// <summary>
        /// Collection of files that are directly contained within this grouping.
        /// </summary>
        public FileReferenceList FileReferences { get; }

        IList<IFileReference> IContentGroup.FileReferences
        {
            get
            {
                FileReferenceListWrapper innerList = _innerList;
                if (innerList == null)
                    _innerList = innerList = new FileReferenceListWrapper(FileReferences);
                return innerList;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual MD5Checksum? BaseChecksum => null;

        MD5Checksum? IContentInfo.Checksum => BaseChecksum;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected ContentGroup() { FileReferences = new FileReferenceList(this); }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<IFileReference> GetAllFileReferences();
    }
}